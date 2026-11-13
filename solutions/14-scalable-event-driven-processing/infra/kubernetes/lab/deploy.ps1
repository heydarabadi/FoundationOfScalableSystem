param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedContext,

    [string]$KindClusterName
)

$ErrorActionPreference = "Stop"

function Invoke-Kubectl {
    param([string[]]$KubectlArgs)

    & kubectl --context $ExpectedContext @KubectlArgs
    if ($LASTEXITCODE -ne 0) {
        throw "kubectl $($KubectlArgs -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Invoke-Docker {
    param([string[]]$DockerArgs)

    & docker @DockerArgs
    if ($LASTEXITCODE -ne 0) {
        throw "docker $($DockerArgs -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Invoke-Dotnet {
    param([string[]]$DotnetArgs)

    & dotnet @DotnetArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($DotnetArgs -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Invoke-Npm {
    param([string[]]$NpmArgs)

    Push-Location (Join-Path $solutionRoot "web")
    try {
        & npm @NpmArgs
        if ($LASTEXITCODE -ne 0) {
            throw "npm $($NpmArgs -join ' ') failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

$currentContext = (& kubectl config current-context).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Could not read the current kubectl context."
}
if ($currentContext -ne $ExpectedContext) {
    throw "Current context is '$currentContext', but ExpectedContext is '$ExpectedContext'. Select the intended context, then run this script again."
}
if ($KindClusterName -and $ExpectedContext -ne "kind-$KindClusterName") {
    throw "KindClusterName '$KindClusterName' normally uses context 'kind-$KindClusterName', but ExpectedContext is '$ExpectedContext'. Refusing to load images into a potentially different cluster."
}

& kubectl --context $ExpectedContext get crd gateways.gateway.networking.k8s.io *> $null
if ($LASTEXITCODE -ne 0) {
    throw "Gateway API CRDs are missing. Install Envoy Gateway v1.10.0 before deploying the HPT lab."
}

$solutionRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
$namespaceFile = Join-Path $solutionRoot "infra\kubernetes\namespace.yaml"
$labRoot = Join-Path $solutionRoot "infra\kubernetes\lab"
$publishRoot = Join-Path $solutionRoot "artifacts\kubernetes"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "The .NET 10 SDK is required to publish the application images on the host."
}
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    throw "Node.js and npm are required to build the web image on the host."
}

Write-Host "Deploying the HPT lab to context '$ExpectedContext'."
Write-Warning "This lab uses development credentials and single-instance Redis/PostgreSQL. Do not use it as a production profile."

Invoke-Kubectl @("apply", "-f", $namespaceFile)
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "metrics-server.yaml"))
Invoke-Kubectl @("wait", "deployment/metrics-server", "-n", "kube-system", "--for=condition=Available", "--timeout=5m")
Invoke-Kubectl @("wait", "apiservice/v1beta1.metrics.k8s.io", "--for=condition=Available", "--timeout=5m")

$initConfigMap = & kubectl --context $ExpectedContext create configmap scalable-event-driven-processing-postgres-init `
    -n scalable-event-driven-processing `
    "--from-file=001-init.sql=$(Join-Path $solutionRoot 'infra\postgres\init.sql')" `
    --dry-run=client `
    -o yaml
if ($LASTEXITCODE -ne 0) {
    throw "Could not generate the PostgreSQL init ConfigMap manifest."
}
$initConfigMap | & kubectl --context $ExpectedContext apply -f -
if ($LASTEXITCODE -ne 0) {
    throw "Could not apply the PostgreSQL init ConfigMap."
}

Invoke-Kubectl @("apply", "-n", "scalable-event-driven-processing", "-f", "https://strimzi.io/install/latest?namespace=scalable-event-driven-processing")
Invoke-Kubectl @("wait", "deployment/strimzi-cluster-operator", "-n", "scalable-event-driven-processing", "--for=condition=Available", "--timeout=10m")
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "kafka.yaml"))
Invoke-Kubectl @("wait", "kafka/scalable-event-driven-processing-kafka", "-n", "scalable-event-driven-processing", "--for=condition=Ready", "--timeout=20m")
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "basket-topic-4p.yaml"))
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "dependencies.yaml"))
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "config.yaml"))
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "secret.yaml"))

Invoke-Dotnet @("publish", (Join-Path $solutionRoot "src\ScalableEventDrivenProcessing.Api\ScalableEventDrivenProcessing.Api.csproj"), "-c", "Release", "-r", "linux-x64", "--self-contained", "false", "-o", (Join-Path $publishRoot "api"), "/p:UseAppHost=false")
Invoke-Dotnet @("publish", (Join-Path $solutionRoot "src\ScalableEventDrivenProcessing.BasketProcessor\ScalableEventDrivenProcessing.BasketProcessor.csproj"), "-c", "Release", "-r", "linux-x64", "--self-contained", "false", "-o", (Join-Path $publishRoot "basket-processor"), "/p:UseAppHost=false")
Invoke-Dotnet @("publish", (Join-Path $solutionRoot "src\ScalableEventDrivenProcessing.InventoryProcessor\ScalableEventDrivenProcessing.InventoryProcessor.csproj"), "-c", "Release", "-r", "linux-x64", "--self-contained", "false", "-o", (Join-Path $publishRoot "inventory-processor"), "/p:UseAppHost=false")
Invoke-Docker @("build", "--load", "-t", "scalable-event-driven-processing-api:dev", "--build-arg", "APP_DLL=ScalableEventDrivenProcessing.Api.dll", "-f", (Join-Path $labRoot "Dockerfile.runtime"), (Join-Path $publishRoot "api"))
Invoke-Docker @("build", "--load", "-t", "scalable-event-driven-processing-basket-processor:dev", "--build-arg", "APP_DLL=ScalableEventDrivenProcessing.BasketProcessor.dll", "-f", (Join-Path $labRoot "Dockerfile.runtime"), (Join-Path $publishRoot "basket-processor"))
Invoke-Docker @("build", "--load", "-t", "scalable-event-driven-processing-inventory-processor:dev", "--build-arg", "APP_DLL=ScalableEventDrivenProcessing.InventoryProcessor.dll", "-f", (Join-Path $labRoot "Dockerfile.runtime"), (Join-Path $publishRoot "inventory-processor"))
Invoke-Npm @("ci", "--no-audit", "--no-fund")
Invoke-Npm @("run", "build")
Invoke-Docker @("build", "--load", "-t", "scalable-event-driven-processing-web:dev", "-f", (Join-Path $solutionRoot "web\Dockerfile.runtime"), (Join-Path $solutionRoot "web"))

if ($KindClusterName) {
    if (-not (Get-Command kind -ErrorAction SilentlyContinue)) {
        throw "KindClusterName was supplied, but the kind command was not found."
    }
    & kind load docker-image scalable-event-driven-processing-api:dev scalable-event-driven-processing-basket-processor:dev scalable-event-driven-processing-inventory-processor:dev scalable-event-driven-processing-web:dev --name $KindClusterName
    if ($LASTEXITCODE -ne 0) {
        throw "Could not load the application images into kind cluster '$KindClusterName'."
    }
}

Invoke-Kubectl @("rollout", "status", "statefulset/scalable-event-driven-processing-postgres", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-redis", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "apps.yaml"))
Invoke-Kubectl @("rollout", "restart", "deployment/scalable-event-driven-processing-api", "-n", "scalable-event-driven-processing")
Invoke-Kubectl @("rollout", "restart", "deployment/scalable-event-driven-processing-basket-processor", "-n", "scalable-event-driven-processing")
Invoke-Kubectl @("rollout", "restart", "deployment/scalable-event-driven-processing-inventory-processor", "-n", "scalable-event-driven-processing")
Invoke-Kubectl @("rollout", "restart", "deployment/scalable-event-driven-processing-web", "-n", "scalable-event-driven-processing")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-api", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-basket-processor", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-inventory-processor", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-web", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "api-gateway.yaml"))
Invoke-Kubectl @("wait", "gateway/scalable-event-driven-processing-gateway", "-n", "scalable-event-driven-processing", "--for=condition=Programmed", "--timeout=5m")
Invoke-Kubectl @("wait", "httproute/scalable-event-driven-processing-api-route", "-n", "scalable-event-driven-processing", "--for=condition=Accepted", "--timeout=5m")

Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "observability-exporters.yaml"))
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "prometheus-rbac.yaml"))
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "prometheus-config.yaml"))
Invoke-Kubectl @("apply", "-f", (Join-Path $labRoot "prometheus.yaml"))
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-prometheus", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-kafka-kafka-exporter", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-postgres-exporter", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("rollout", "status", "deployment/scalable-event-driven-processing-redis-exporter", "-n", "scalable-event-driven-processing", "--timeout=10m")
Invoke-Kubectl @("get", "pods,services,hpa", "-n", "scalable-event-driven-processing", "-o", "wide")

Write-Host "Lab deployment is available. Web: kubectl --context $ExpectedContext port-forward -n scalable-event-driven-processing svc/scalable-event-driven-processing-web 8081:80"
Write-Host "Prometheus: kubectl --context $ExpectedContext port-forward -n scalable-event-driven-processing svc/scalable-event-driven-processing-prometheus 19090:9090"
