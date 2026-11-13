<div dir="rtl" align="right">

# راهنمای هم‌روندی، زبان‌های برنامه‌نویسی و رویکردهای اجرا

این راهنما مکمل فصل ۴ و معیار مقایسهٔ زبان‌ها در کل مخزن است. زبان سریع مطلق وجود ندارد: workload، library، driver، allocation، serialization، شبکه، پایگاه‌داده و کیفیت پیاده‌سازی ممکن است نتیجه را عوض کنند. هدف، انتخاب مدل هم‌روندی متناسب و ساخت benchmark منصفانه است.

## ۱. پنج مفهوم پایه

- **Concurrency:** چند کار در یک بازه پیشرفت می‌کنند؛ لازم نیست هم‌زمان روی چند هسته باشند.
- **Parallelism:** چند کار واقعاً هم‌زمان روی چند واحد پردازش اجرا می‌شوند.
- **I/O-bound:** زمان عمده در انتظار شبکه، دیسک یا دیتابیس می‌گذرد؛ async یا concurrency بیشتر می‌تواند threadهای مسدود را آزاد کند.
- **CPU-bound:** زمان عمده صرف محاسبه می‌شود؛ async به‌تنهایی سرعت نمی‌دهد و موازی‌سازی، الگوریتم، vectorization یا native code بررسی می‌شود.
- **Backpressure:** وقتی مصرف‌کننده کندتر است، تولیدکننده باید کند، منتظر، reject یا drop کند؛ صف نامحدود مشکل را به کمبود حافظه تبدیل می‌کند.

## ۲. رویکردهای طراحی هم‌روندی

| رویکرد | مدل مالکیت/هماهنگی | قوت | ریسک اصلی |
|---|---|---|---|
| Shared memory + mutex/lock | چند task به state مشترک دسترسی دارند | مناسب invariantهای مشترک و دادهٔ محلی | race، deadlock، contention و ناحیهٔ سری |
| Actor / mailbox | هر actor مالک state است؛ پیام دریافت می‌کند | کاهش state مشترک و جداسازی failure | mailbox رشد می‌کند؛ ordering و supervision لازم است |
| CSP / channels | انتقال داده و هماهنگی با channel | pipeline و producer-consumer واضح | channel مسدود/نامحدود و ownership نادرست |
| Event loop | یک loop callbackهای غیرمسدودکننده را اجرا می‌کند | I/O زیاد با thread کم | callback CPU-heavy همهٔ درخواست‌ها را عقب می‌اندازد |
| Async/await | task هنگام I/O suspend می‌شود، نه thread | composition خوانا و بهره‌وری thread | async غیرهم‌زمانی CPU نیست؛ blocking call گلوگاه است |
| Work stealing/thread pool | taskها در pool اجرا می‌شوند | بهره‌وری هسته برای کار CPU و taskها | اندازه، queueing و starvation باید کنترل شوند |
| Message broker | workerهای مستقل از طریق پیام durable جدا می‌شوند | جذب burst، retry و scale مستقل | eventual consistency، lag، duplicate و عملیات broker |

اصل عملی: اول state مشترک را کم کن؛ concurrency را با محدودیت منابع و deadline ببند؛ بعد با اندازه‌گیری سطح parallelism را بالا ببر.

## ۳. زبان‌ها و runtimeها

| زبان/runtime | مدل رایج | مناسب برای | چه چیزی را مقایسه کن |
|---|---|---|---|
| C# / .NET | async/await، Task، thread pool، Channel<T> | API و worker I/O، stack فعلی مخزن | allocation، thread-pool starvation، blocking، connection pool، queue bound |
| Go | goroutine + channel، mutex وقتی لازم است | سرویس و pipeline concurrent ساده | allocation/GC، goroutine leak، channel backpressure، scheduler و pprof |
| Rust | thread، async runtime و ownership/type system | hot path، کنترل حافظه و کار CPU حساس | زمان توسعه، async runtime، ownership هزینهٔ مهندسی، allocator و safety |
| Java / Scala JVM | thread pools، Futures، ecosystem Kafka/Flink؛ Java virtual threads | JVM services و stream/data pipeline | warm-up/JIT، GC، blocking، JVM memory و driver behavior |
| Elixir / BEAM | processهای سبک، message passing و supervision | تعداد زیاد کار I/O و سرویس fault-isolated | throughput CPU-heavy، mailbox رشدکرده، binary copying و ecosystem/database driver |
| Node.js | event loop برای JS، async I/O و worker pool/thread برای CPU | gateway و I/O-heavy API | event-loop lag، callback طولانی، worker pool saturation و memory |

این‌ها رتبه‌بندی نیستند. برای workload CPU-heavy، Elixir یا Node را بدون اندازه‌گیری در جایگزینی زبان native فرض نکن؛ برای workload I/O-heavy، زبان‌هایی با blocking مدل هم می‌توانند با runtime خوب throughput بالا بدهند. Java virtual threads برای taskهای عمدتاً منتظر I/O طراحی شده‌اند و thread سریع‌تر محسوب نمی‌شوند.

## ۴. هم‌روندی عمیق در .NET، Go و Rust

این سه زبان هر سه می‌توانند تعداد زیادی کار را هم‌زمان پیش ببرند، اما ابزارها و مرزهایشان یکسان نیست. «یک کار هم‌روند» لزوماً مساوی یک thread سیستم‌عامل نیست؛ و زیاد کردن تعداد taskها یا goroutineها هم ظرفیت CPU، Kafka یا دیتابیس را زیاد نمی‌کند. برای انتخاب درست، باید مدل اجرا، مالکیت state، صف، محدودیت منابع، لغو و نحوهٔ گزارش خطا را با هم ببینیم.

### ۴.۱ واژه‌های مشترک: Task، thread، goroutine و Future

| واژه | معنی کاربردی | تضمین نمی‌کند |
|---|---|---|
| OS thread | مسیر اجرایی زمان‌بندی‌شده توسط سیستم‌عامل | که هر کار برنامه یک thread مستقل لازم دارد |
| .NET `Task` | نمایندهٔ یک عملیات که ممکن است هنوز تمام نشده باشد؛ معمولاً با `async`/`await` ترکیب می‌شود | که حتماً روی thread جدا اجرا شود |
| Go goroutine | تابعی که runtime هم‌زمان با goroutineهای دیگر زمان‌بندی می‌کند | که ارزان بودن شروع آن به معنی مجاز بودن تعداد نامحدود باشد |
| Rust `Future` | توصیف محاسبه‌ای که با poll شدن پیش می‌رود | که خودبه‌خود اجرا شود؛ برای اجرا به executor/runtime نیاز دارد |
| parallelism | اجرای واقعاً هم‌زمان روی چند هسته | که برای هر workload سریع‌تر باشد؛ contention و هماهنگی هم هزینه دارند |

در مسیر I/O، هدف معمولاً آزاد کردن thread یا runtime worker هنگام انتظار برای شبکه و دیتابیس است. در مسیر CPU، کار باید روی هسته‌ها تقسیم شود؛ `async` به‌تنهایی محاسبه را موازی نمی‌کند. در هر سه زبان باید تعداد کارهای هم‌زمان را با ظرفیت محدودترین وابستگی تنظیم کرد.

### ۴.۲ .NET / C#

#### مدل اجرا

`async`/`await` الگوی نوشتن عملیات asynchronous را فراهم می‌کند. برای I/O واقعاً asynchronous مثل `HttpClient.SendAsync` یا API ناهمگام دیتابیس، هنگام انتظار thread لازم نیست همان‌جا مسدود بماند؛ ادامهٔ عملیات بعداً اجرا می‌شود. `Task` یک thread نیست. `Task.Run` برای انتقال محاسبهٔ CPU-bound به ThreadPool می‌تواند مناسب باشد، اما دور یک API مسدودکننده پیچیدن `Task.Run` آن API را واقعاً non-blocking نمی‌کند و ممکن است ThreadPool را اشباع کند. ThreadPool threadها را میان کارهای کوتاه بازاستفاده می‌کند.

#### state مشترک و هماهنگی

- اگر ممکن است، داده را به عملیات بده و نتیجه را برگردان؛ state mutable مشترک را کم کن.
- برای state مشترک هم‌زمان، از `lock`/`Monitor`, `SemaphoreSlim`, `Interlocked` یا ساختارهای concurrent متناسب استفاده کن؛ lock را هنگام `await` نگه ندار مگر طراحی و طول critical section کاملاً روشن باشد.
- `Channel<T>` برای صف producer-consumer مناسب است. نسخهٔ bounded ظرفیت مشخص دارد و در حالت `Wait`، نویسنده وقتی صف پر شود منتظر می‌ماند؛ این backpressure است.
- صف bounded فقط تعداد اقلام را محدود می‌کند. اگر هر پیام بزرگ است، حد حافظه را با اندازهٔ payload هم حساب کن.

#### نمونه: صف محدود و چند worker

این نمونه نشان می‌دهد صف کجا محدود می‌شود و workerها چطور پیام را تا پایان پردازش مصرف می‌کنند. `ProcessAsync` در برنامهٔ واقعی باید به عملیات idempotent و cancellation-aware متصل شود.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public sealed record Job(string Id);

public static class WorkerPool
{
    public static async Task RunAsync(
        IAsyncEnumerable<Job> input,
        int workerCount,
        CancellationToken stoppingToken)
    {
        if (workerCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(workerCount));

        var channel = Channel.CreateBounded<Job>(new BoundedChannelOptions(256)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        });

        async Task ProduceAsync()
        {
            try
            {
                await foreach (var job in input.WithCancellation(stoppingToken))
                    await channel.Writer.WriteAsync(job, stoppingToken);
            }
            finally
            {
                channel.Writer.TryComplete();
            }
        }

        async Task ConsumeAsync()
        {
            await foreach (var job in channel.Reader.ReadAllAsync(stoppingToken))
                await ProcessAsync(job, stoppingToken);
        }

        var workers = Enumerable.Range(0, workerCount)
            .Select(_ => ConsumeAsync())
            .ToArray();

        var producer = ProduceAsync();
        await Task.WhenAll(workers.Append(producer));
    }

    private static Task ProcessAsync(Job job, CancellationToken cancellationToken)
    {
        // جایگزین با منطق واقعی؛ token را به DB/HTTP/Kafka client هم بده.
        return Task.CompletedTask;
    }
}
```

در نسخهٔ تولیدی، رفتار خطای worker را صریح کن: آیا خطا باید producer را لغو کند؟ آیا پیام به صف durable برمی‌گردد؟ آیا worker روی poison message متوقف می‌شود یا آن را به DLQ می‌فرستد؟ در مثال بالا، یک exception در taskها به `Task.WhenAll` می‌رسد؛ اما منبع ورودی درون‌حافظه‌ای است و تضمین دوام پیام ندارد.

#### لغو و shutdown در .NET

`CancellationToken` یک درخواست لغو هماهنگ و تعاونی است، نه قطع اجباری thread. توکن را از `BackgroundService.ExecuteAsync(stoppingToken)` به حلقه، `ReadAllAsync`, `WriteAsync` و عملیات پشتیبان منتقل کن. هنگام shutdown معمولاً این ترتیب لازم است:

1. intake پیام تازه متوقف شود.
2. صف داخلی کامل شود و producer دیگر آیتم جدید اضافه نکند.
3. پیام‌های در حال اجرا تا مهلت graceful shutdown تمام شوند یا با cancellation وارد مسیر بازیابی شوند.
4. فقط پیام‌هایی که اثرشان پایدار و موفق شده commit شوند.
5. client و اتصال‌ها بسته شوند.

لغو را swallow نکن؛ `OperationCanceledException` ناشی از همان token را با خطای کاری اشتباه نگیر. مهلت shutdown باید با `terminationGracePeriodSeconds`، timeout کلاینت‌ها و زمان drain هماهنگ باشد.

#### خطاهای معمول .NET

- `.Result`, `.Wait()` یا I/O همگام داخل مسیر async می‌تواند thread را مسدود و ThreadPool starvation ایجاد کند.
- `Task.Run` برای هر درخواست بدون سقف، فقط صف دیگری می‌سازد و مصرف حافظه را بالا می‌برد.
- `async void` به جز handlerهای event معمولاً خطاها و completion را درست قابل انتظار نمی‌کند.
- فراموش کردن `await` باعث می‌شود خطا یا پایان عملیات از دید فراخواننده جدا شود.
- `CancellationToken` اگر به driver یا API downstream پاس داده نشود، لغو فقط ظاهری است.
- `Channel` با `DropOldest/DropWrite` ممکن است داده را عمداً حذف کند؛ برای فرمان مالی یا پیام durable، چنین سیاستی بدون قرارداد صریح خطرناک است.

#### پایش و ابزار

ThreadPool queue length، تعداد threadها، زمان انتظار صف، طول `Channel`, نرخ لغو و خطا، connection pool، latency و allocation/GC را ثبت کن. از `dotnet-counters`, `dotnet-trace` و profiler برای تشخیص CPU، contention و ThreadPool starvation استفاده کن؛ میانگین تنها کافی نیست و p95/p99 را هم بسنج.

### ۴.۳ Go

#### مدل اجرا

`go f()` یک goroutine ایجاد می‌کند. Go runtime تعداد زیادی goroutine را روی تعداد کمتری OS thread multiplex می‌کند؛ goroutine کوچک‌تر از thread معمولی است، اما همچنان stack، state، زمان‌بندی و منابع downstream هزینه دارد. اصل رایج Go این است که اگر مناسب بود با channel پیام و مالکیت داده را منتقل کن؛ برای state کوچک و مشترکی مثل شمارنده یا cache، mutex هم گاهی ساده‌تر و روشن‌تر است. channel به خودی خود مسئله را حل نمی‌کند: مالکیت channel، بسته‌شدنش و لغو کار باید تعریف شود.

#### state، channel و محدودیت

- `chan T` با buffer محدود، صف و نقطهٔ هماهنگی می‌سازد؛ وقتی buffer پر باشد، ارسال‌کننده backpressure می‌گیرد.
- `sync.Mutex` و `sync.RWMutex` برای invariantهای مشترک؛ `sync/atomic` برای عملیات اتمی مشخص؛ `WaitGroup` برای انتظار پایان گروهی از کارها.
- `context.Context` برای deadline و cancellation که از درخواست یا supervisor به عملیات پایین‌دست منتقل می‌شود. تابعی که لغو می‌سازد باید `cancel()` را اجرا کند؛ سرویس نباید context را در struct سراسری نگه دارد.
- goroutineها خودشان garbage-collected نمی‌شوند تا وقتی تمام شوند؛ goroutine گیرکرده روی channel یا I/O می‌تواند leak باشد.

#### نمونه: worker pool با صف bounded و cancellation

```go
package workers

import (
    "context"
    "errors"
    "sync"
)

type Job struct{ ID string }

func Run(ctx context.Context, input <-chan Job, workerCount int,
    process func(context.Context, Job) error) error {
    if workerCount <= 0 {
        return errors.New("workerCount must be positive")
    }

    ctx, cancel := context.WithCancel(ctx)
    defer cancel()

    jobs := make(chan Job, 256)
    errs := make(chan error, workerCount)

    var wg sync.WaitGroup
    for i := 0; i < workerCount; i++ {
        wg.Add(1)
        go func() {
            defer wg.Done()
            for {
                select {
                case <-ctx.Done():
                    return
                case job, ok := <-jobs:
                    if !ok {
                        return
                    }
                    if err := process(ctx, job); err != nil {
                        select {
                        case errs <- err:
                        default:
                        }
                        cancel()
                        return
                    }
                }
            }
        }()
    }

feed:
    for {
        select {
        case <-ctx.Done():
            break feed
        case job, ok := <-input:
            if !ok {
                break feed
            }
            select {
            case jobs <- job: // اگر صف پر باشد، producer منتظر می‌ماند.
            case <-ctx.Done():
                break feed
            }
        }
    }

    close(jobs) // فقط مالک producer این channel را می‌بندد.
    wg.Wait()
    close(errs)
    for err := range errs {
        if err != nil {
            return err
        }
    }
    return ctx.Err()
}
```

نمونه برای آموزش worker pool است؛ در سیستم واقعی مشخص کن بعد از اولین خطا بقیهٔ کارهای صف‌شده drain شوند یا متوقف شوند، و cancellation را به `process` و هر درخواست DB/HTTP بده. `ctx.Err()` بعد از cancel شدن خطا برمی‌گرداند؛ در برنامهٔ واقعی معمولاً نتیجهٔ `context.Canceled` را از خطای کاری جدا می‌کنی. برای Kafka، اگر پردازش لغو شد، offset پیام ناتمام را جلو نبر تا broker بتواند آن را دوباره تحویل دهد.

#### خطاهای معمول Go

- ساخت goroutine بدون مسیر قطعی پایان، leak ایجاد می‌کند؛ برای هر goroutine بپرس «چه چیزی باعث خروجش می‌شود؟».
- producer و consumer هر دو channel را نبندند؛ مالک ارسال باید channel را ببندد. ارسال به channel بسته panic می‌کند.
- `select` با چند case آماده تضمین اولویت ندارد؛ اگر deadline مهم است، رفتار و بررسی نهایی context را طراحی کن.
- `for range` روی channel تا بسته‌شدن آن منتظر می‌ماند؛ اگر هیچ‌کس آن را نبندد workerها متوقف نمی‌شوند.
- دسترسی هم‌زمان به map عادی بدون lock مجاز نیست؛ ابزار race detector را در تست/اجرای هدفمند فعال کن.
- `GOMAXPROCS` یا تعداد goroutineها جای محدودکردن DB connection و درخواست‌های خارجی را نمی‌گیرند.

#### پایش و ابزار

از `go test -race ./...` برای پیدا کردن raceهای قابل‌بازآفرینی در تست‌ها، و `pprof` برای CPU، heap، goroutine و block profile استفاده کن. تعداد goroutineهای زنده، channel queueing، زمان انتظار، زمان سرویس، GC، DB pool، Kafka lag و نرخ cancellation را جدا ببین. Race detector proof کامل نبودن race نیست؛ فقط مسیرهایی را می‌یابد که اجرا شده‌اند.

### ۴.۴ Rust

#### دو مدل اجرایی را از هم جدا کن

در Rust هم threadهای سیستم‌عامل در `std::thread` داریم و هم async. در async، یک `Future` فقط توضیح می‌دهد مرحلهٔ بعد چیست و برای پیشرفت باید runtime آن را poll کند؛ Tokio یکی از runtimeهای رایج برای I/O و شبکه است. یک future async که CPU طولانی را بدون `await` انجام دهد، worker runtime را مشغول می‌کند و کارهای دیگر همان executor را عقب می‌اندازد. برای محاسبهٔ CPU-heavy از pool محدود مانند Rayon یا بخش blocking مدیریت‌شدهٔ runtime استفاده کن؛ تعداد `spawn_blocking`ها را هم کنترل کن.

Rust ownership/borrowing و traitهای `Send` و `Sync` بخشی از انتقال دادهٔ امن میان threadها را در compile time بررسی می‌کنند. این‌ها مانع همهٔ هم‌زمانی‌های نادرست نیستند: deadlock، starvation، ترتیب نادرست، duplicate effect و race منطقی همچنان ممکن‌اند. `Arc<Mutex<T>>` امن از نظر memory safety می‌تواند از نظر طراحی contention یا deadlock داشته باشد.

#### state و channel

- برای threadهای sync، `std::thread`, `Mutex`, `RwLock`, `Condvar` و channelهای `std::sync::mpsc`/`crossbeam-channel` قابل استفاده‌اند.
- در Tokio، `tokio::sync::mpsc::channel(capacity)` bounded است؛ وقتی پر شد `send().await` منتظر ظرفیت می‌ماند.
- bounded کردن queue و workerها مهم‌تر از ساخت task به تعداد پیام‌هاست.
- در async مسیر معمولاً به `Arc` نیاز پیدا می‌کنی فقط وقتی واقعاً state بین taskها مشترک است؛ ownership انتقالی ساده‌تر و کم‌هزینه‌تر است.

#### نمونه: bounded worker pool با Tokio

برای نمونه، در `Cargo.toml` می‌توان dependencyهای runtime و token را این‌طور تعریف کرد (نسخه‌ها را در پروژه pin و به‌روزرسانی کن):

```toml
[dependencies]
tokio = { version = "1", features = ["macros", "rt-multi-thread", "sync", "time"] }
tokio-util = { version = "0.7", features = ["rt"] }
```

نمونهٔ زیر صف را bounded می‌کند و تمام taskها را join می‌کند. `process` جایگزین با منطق برنامه است.

```rust
use std::{error::Error, sync::Arc};
use tokio::{sync::{mpsc, Mutex}, task::JoinSet};

#[derive(Debug)]
struct Job {
    id: String,
}

type AppError = Box<dyn Error + Send + Sync>;

async fn process(_job: Job) -> Result<(), AppError> {
    // DB/Kafka/HTTP async client را اینجا await کن.
    Ok(())
}

async fn run(input: Vec<Job>, worker_count: usize) -> Result<(), AppError> {
    if worker_count == 0 {
        return Err(std::io::Error::new(
            std::io::ErrorKind::InvalidInput,
            "worker_count must be positive",
        ).into());
    }

    let (tx, rx) = mpsc::channel::<Job>(256);
    let shared_rx = Arc::new(Mutex::new(rx));
    let mut workers = JoinSet::new();

    for _ in 0..worker_count {
        let rx = Arc::clone(&shared_rx);
        workers.spawn(async move {
            loop {
                // قفل فقط برای گرفتن آیتم است؛ هنگام پردازش نگهش نمی‌داریم.
                let next = { rx.lock().await.recv().await };
                match next {
                    Some(job) => process(job).await?,
                    None => break,
                }
            }
            Ok::<(), AppError>(())
        });
    }

    for job in input {
        tx.send(job)
            .await
            .map_err(|_| std::io::Error::new(
                std::io::ErrorKind::BrokenPipe,
                "all workers have stopped",
            ))?;
    }
    drop(tx); // پس از تمام شدن producer، workerها صف را drain می‌کنند.

    while let Some(joined) = workers.join_next().await {
        joined??;
    }
    Ok(())
}
```

این مثال الگوی queue و join را نشان می‌دهد و برای production کامل نیست: اگر worker با خطا خارج شود، producer باید از آن باخبر شود تا روی صف پر برای همیشه منتظر نماند. در سرویس واقعی، یک supervisor و cancellation token مشترک تعریف کن؛ با اولین خطای fatal تولید را متوقف کن، senderها را drop کن، workerهای باقی‌مانده را drain یا cancel کن، و همهٔ `JoinHandle`ها را جمع‌آوری کن. وقتی یک branch در `select!` لغو می‌شود، future همان branch drop می‌شود؛ اما **drop کردن `JoinHandle` به‌تنهایی task اجراشده را لغو نمی‌کند و آن را detach می‌کند**. برای taskهای spawnشده از cancellation تعاونی یا `abort`/`abort_all` استفاده کن و سپس پایان آن‌ها را join کن. هر عملیات خارجی ممکن است قبلاً اثر خود را انجام داده باشد، پس idempotency و deadline لازم است. برای Kafka، commit تنها بعد از اثر پایدار و با رعایت ترتیب offsetها انجام شود.

#### خطاهای معمول Rust

- ساخت future بدون `.await` یا spawn به معنی اجرای خودکار آن نیست.
- اجرای CPU سنگین در async worker باعث می‌شود taskهای شبکه‌ای فرصت poll نداشته باشند.
- گرفتن mutex و نگه‌داشتن guard در طول `.await` می‌تواند contention یا deadlock بسازد؛ بخش بحرانی را کوتاه نگه دار.
- `JoinHandle`ها را رها نکن اگر لازم است پایان و panic taskها را مشاهده کنی؛ join و error propagation جزئی از lifecycle است.
- `select!`، cancellation و drop ممکن است future را در نقطه‌ای متوقف کنند که effect خارجی مبهم باشد؛ transaction/idempotency پاسخ این مسئله است، نه فرض «لغو یعنی هیچ اتفاقی نیفتاد».
- compiler جلوی بعضی data raceهای حافظه‌ای را می‌گیرد، اما business invariant، ordering و چند اجرای هم‌زمان یک فرمان را خودت باید کنترل کنی.

#### پایش و ابزار

زمان صف، تعداد taskهای فعال، تعداد اتصال‌های DB، latency و خطای هر مرحله را با tracing ثبت کن. در Tokio از ابزارهای `tracing` و `tokio-console` برای مشاهدهٔ taskها و زمان‌بندی استفاده کن؛ برای CPU و allocation نیز profilerهای native سیستم را به کار ببر. بنچمارک release بساز و نسخهٔ compiler، featureها، allocator، runtime و تنظیم workerها را ثبت کن.

### ۴.۵ مقایسهٔ تصمیم‌گیری

| پرسش | .NET | Go | Rust |
|---|---|---|---|
| عملیات I/O فراوان | `async`/`await` و async client؛ threadهای مسدود را کم کن | goroutine + APIهای context-aware؛ goroutineها را به کار/منبع محدود کن | runtime مثل Tokio + async client همان ecosystem |
| CPU موازی | `Parallel`, محدودیت worker یا `Task.Run` برای محاسبه؛ نه برای I/O | worker pool و تنظیم parallelism مناسب هسته‌ها | thread pool/Rayon یا blocking pool محدود؛ از CPU داخل async executor پرهیز |
| صف و backpressure | bounded `Channel<T>` با FullMode آگاهانه | channel buffered محدود یا semaphore | bounded Tokio mpsc یا bounded sync channel |
| توقف | token تعاونی؛ به همهٔ APIها پاس بده و taskها را await کن | context + close channel + `WaitGroup` | cancellation/supervision صریح، drop/abort با join و رعایت effect |
| محافظت حافظه | GC؛ مراقبت از allocation و object retention | GC؛ مراقبت از heap و goroutine leak | ownership و borrow checker برای بخش مهمی از ایمنی حافظه؛ بررسی منطقی هنوز لازم |
| تشخیص خطا | exception روی Task، cancellation مجزا، counters/traces | error صریح، race detector، pprof | `Result`, join error, tracing و profiler |

هیچ‌کدام به‌تنهایی انتخاب برتر نیستند. .NET انتخاب عملی برای مخزن فعلی و integration با سرویس‌های موجود است؛ Go برای سرویس‌های سادهٔ concurrent با runtime و deployment سبک گزینه‌ای مناسب است؛ Rust می‌تواند در مسیرهای نیازمند کنترل دقیق memory/CPU مفید باشد اما پیچیدگی مهندسی، ecosystem driverها و زمان توسعه هم هزینهٔ واقعی‌اند. تصمیم نهایی را با workload یکسان و semantics یکسان بگیر.

### ۴.۶ الگوی مشترک برای Kafka consumer و shutdown

این قواعد مستقل از زبان‌اند و برای workerهای مخزن مهم‌اند:

1. **Partition مرز ترتیب و موازی‌سازی Kafka است.** در یک consumer group، هر partition در یک زمان به حداکثر یک consumer عضو گروه اختصاص داده می‌شود. بالا بردن تعداد worker از تعداد partitionها، برای همان group موازی‌سازی بیشتری از Kafka ایجاد نمی‌کند؛ مصرف‌کنندهٔ اضافه ممکن است بیکار بماند.
2. **موازی‌سازی داخلی را از موازی‌سازی partition جدا کن.** اگر هر consumer پیام‌ها را داخل چند task موازی پردازش کند، ممکن است پیام offset بالاتر زودتر تمام شود. commit offset بالاتر از پیام‌های ناتمام قبلی باعث رد شدن آن پیام‌ها پس از restart می‌شود؛ commit باید فقط تا بالاترین offset پیوستهٔ تکمیل‌شده جلو برود، یا پردازش هر partition ترتیبی بماند.
3. **اثر business و offset را هماهنگ کن.** وقتی DB و Kafka یک transaction مشترک ندارند، معمولاً ابتدا اثر durable را با idempotency ثبت می‌کنند و بعد offset را commit می‌کنند. اگر process بین این دو مرحله crash کند، پیام دوباره می‌آید و اثر idempotent باید duplicate را بی‌خطر کند.
4. **Backpressure را تا broker منتقل کن.** اگر صف داخلی پر است، producer مصرف‌کننده نباید بدون حد پیام بیشتر در حافظه انباشته کند. گزینه‌ها شامل pause پارتیشن، محدودکردن fetch/worker یا انتظار در bounded queue هستند؛ جزئیات به Kafka client بستگی دارد.
5. **Shutdown را به ترتیب طراحی کن.** دریافت تازه را متوقف کن؛ کارهای in-flight را تا deadline drain کن؛ اثرهای موفق را ثبت و offsetهای پیوستهٔ تکمیل‌شده را commit کن؛ سپس consumer را ببند. اگر deadline گذشت، پیام ناقص باید قابل replay باشد؛ Kubernetes نمی‌فهمد پیام business واقعاً تمام شده است.
6. **ظرفیت را ضرب کن.** `replicaهای Pod × worker به‌ازای Pod` حداکثر پردازش هم‌زمان برنامه است، اما ظرفیت واقعی با partitionها، CPU request/limit، DB pool، broker، external API و ترتیب key محدود می‌شود. افزایش worker وقتی DB pool ثابت است می‌تواند latency و timeout را بدتر کند.

### ۴.۷ چک‌لیست طراحی و بازبینی

- کار CPU-bound است یا منتظر I/O؟ profiler و trace چه چیزی نشان می‌دهند؟
- workerها و صف چه سقفی دارند؟ اگر صف پر شد، producer wait می‌کند، رد می‌کند، drop می‌کند یا durable buffer می‌سازد؟
- هر state و key را کدام worker مالک است؟ invariant و ordering در چه محدوده‌ای باید برقرار بماند؟
- cancellation تا آخرین API و query می‌رسد؟ timeout کل request از جمع timeoutهای پایین‌دست بزرگ‌تر نشده؟
- بعد از اولین خطا، worker pool چه می‌کند؟ retry، DLQ، fail-fast یا ادامهٔ کار؟ خطای ثانویه کجا ثبت می‌شود؟
- هنگام shutdown، intake کجا بسته می‌شود، صف کجا drain می‌شود و offset کجا commit می‌شود؟ deadlineها با هم سازگارند؟
- ظرفیت poolهای DB/HTTP/Kafka با تعداد workerها هماهنگ است؟ آیا scale-out باعث رقابت بیشتر برای یک منبع مشترک می‌شود؟
- چه metricهایی queue depth، queue wait، active workers، in-flight count، timeout، retry و completion را نشان می‌دهند؟
- آیا load test اثر پایدار کسب‌وکار را می‌شمارد یا فقط request پذیرفته‌شده را؟

### ۴.۸ تمرین عملی پیشنهادی

یک سناریوی یکسان «دریافت فرمان سبد خرید، اعتبارسنجی، ثبت idempotent در PostgreSQL و commit offset Kafka» را برای .NET، Go و Rust پیاده‌سازی کن. قرارداد پیام، partition key، تعداد partition، replication، DB schema، اندازهٔ pool، retry و رفتار خطا را ثابت نگه دار. برای هر نسخه این گام‌ها را انجام بده:

1. ابتدا تست صحت و idempotency را بدون بار اجرا کن.
2. صف bounded و worker count قابل تنظیم بساز؛ حالت صف پر را در log/metric ببین.
3. بار را مرحله‌ای بالا ببر و throughput موفق، p50/p95/p99، CPU، RAM، GC/allocation، queue wait، DB pool wait، Kafka lag و error rate را ثبت کن.
4. consumer را حین کار terminate کن؛ بررسی کن intake متوقف شده، پیام‌های موفق commit شده‌اند، پیام‌های ناتمام replay شده‌اند و اثر تکراری نداری.
5. با partition count و DB pool ثابت، worker count را بالا ببر و نقطه‌ای را پیدا کن که throughput دیگر رشد نمی‌کند یا tail latency بدتر می‌شود.

گزارش هر variant باید شامل نسخهٔ compiler/runtime و library client، تنظیمات، سخت‌افزار، محدودیت container، دادهٔ warm-up و نتیجهٔ صحت باشد. benchmark تنها وقتی مقایسه‌پذیر است که semanticها و سخت‌افزار برابر باشند.

### ۴.۹ Elixir — نمونهٔ مکمل برای I/O زیاد

```elixir
items
|> Task.async_stream(&process/1,
  max_concurrency: worker_count,
  ordered: false,
  timeout: 5_000
)
|> Stream.run()
```

`max_concurrency` را به توان دیتابیس و سرویس خارجی محدود کن. mailbox یا task نامحدود می‌تواند حافظه را رشد دهد؛ process سبک به معنی ظرفیت نامتناهی نیست. این مثال مکمل است؛ تمرکز عمیق این بخش روی .NET، Go و Rust است.

## ۵. برنامهٔ benchmark منصفانه

یک سناریو را در همهٔ زبان‌ها ثابت نگه دار:

1. API قرارداد و schema پیام ثابت، مثلاً فرمان تغییر سبد با `basketId`, `requestId`, `version`.
2. همان Kafka/database، topic، partition key، replication، payload، batch و retention.
3. semantics یکسان: retry، idempotency، commit پس از اثر پایدار، ordering و error handling.
4. warm-up، ramp-up، مدت آزمون، تعداد client، سخت‌افزار و resource limit ثابت.
5. آزمون درست‌بودن قبل و بعد از بار: شمار پیام پذیرفته‌شده، پردازش‌شده، duplicate، خطا و اختلاف projection.
6. ثبت throughput موفق، p50/p95/p99، CPU، RAM، GC/alloc، event-loop lag، queue wait، Kafka lag، DB pool/locks و زمان recovery.

یک زبان را با batch بزرگ‌تر یا semantics ضعیف‌تر «برنده» اعلام نکن. هم هزینهٔ CPU و RAM را گزارش کن، هم ساده‌بودن عملیات و زمان پیاده‌سازی را.

## ۶. مسیرهای پروژهٔ قابل‌پیاده‌سازی

1. **.NET baseline:** API async + PostgreSQL تراکنشی؛ ابزارگذاری latency و connection pool.
2. **Go worker:** همان Kafka topic/key، consumer group و database contract؛ worker pool محدود، cancellation و profiling.
3. **Rust hot path:** فقط مرحلهٔ CPU-heavy serialization/rules را جدا port کن؛ ابتدا FFI/complexity هزینه را با سود واقعی مقایسه کن.
4. **Scala/Flink stream:** aggregation یا window واقعی از Kafka؛ event time، checkpoint، late data و sink semantics را مشخص کن.
5. **Elixir I/O workers:** یک سرویس مستقل برای اتصال‌های زیاد یا کارهای پیام‌محور؛ supervision و محدودیت concurrency را طراحی و بار واقعی را مقایسه کن.
6. **Node.js gateway:** I/O fan-out و routing؛ CPU سنگین را از event loop خارج کن و event-loop delay را پایش کن.

همهٔ variantها باید مستقل باشند و کد اجرایی را از هم کپی نکنند؛ فقط contract و workload در اسناد مشترک تعریف شود.

## ۷. پرسش‌های بازبینی

- کار ما CPU-bound است یا I/O-bound؟ شواهد profiler چیست؟
- state هر کلید را کدام worker مالک است؟ ترتیب لازم در چه محدوده‌ای است؟
- سقف صف و رفتار پرشدن آن چیست؟ cancellation چطور تا DB/Kafka می‌رسد؟
- آیا scale-out worker بدون افزایش pool مشترک database فقط ازدحام می‌سازد؟
- آیا رقمی که اعلام می‌کنیم throughput پذیرفته‌شده است یا اثر کسب‌وکار نهایی؟

## تمرین مقایسهٔ زبان‌ها

یک worker واحد با قرارداد ثابت پیام و effect idempotent در .NET و Go بساز؛ سپس بخش CPU-heavy یا event-time را در Rust یا Scala/Flink و یک مسیر I/O-concurrency را در Elixir یا Node.js بررسی کن. همه باید از schema، partition key، database، retry و commit یکسان استفاده کنند. سه سطح بار و concurrency را اجرا کن، بعد worker را حین کار متوقف و recovery را بسنج. در گزارش throughput موفق، p99، errors، CPU، memory/GC/allocation، queue wait، lag و زمان توسعه/عملیات را بیاور. اگر یک نسخه semantics متفاوت یا batch بزرگ‌تر دارد، عددهایش قابل‌مقایسه نیستند.

## منابع رسمی

- [.NET asynchronous scenarios](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-scenarios)، [managed ThreadPool](https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool)، [cooperative cancellation](https://learn.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads)، و [Channels/backpressure](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)
- [Go Effective Go: concurrency](https://go.dev/doc/effective_go#concurrency)، [Go memory model](https://go.dev/ref/mem)، [context package](https://pkg.go.dev/context)، [race detector](https://go.dev/doc/articles/race_detector)، و [cancel database operations](https://go.dev/doc/database/cancel-operations)
- [Rust: Fearless Concurrency](https://doc.rust-lang.org/book/ch16-00-concurrency.html)، [Tokio tutorial](https://tokio.rs/tokio/tutorial)، [Tokio graceful shutdown](https://tokio.rs/tokio/topics/shutdown)، [Tokio `JoinHandle` lifecycle](https://docs.rs/tokio/latest/tokio/task/struct.JoinHandle.html)، [Tokio `CancellationToken`](https://docs.rs/tokio-util/latest/tokio_util/sync/struct.CancellationToken.html)، و [Tokio bounded mpsc/backpressure](https://docs.rs/tokio/latest/tokio/sync/mpsc/)
- [Java virtual threads](https://docs.oracle.com/en/java/javase/21/core/virtual-threads.html)
- [Scala Futures](https://docs.scala-lang.org/overviews/core/futures.html)
- [Elixir processes](https://elixir-lang.org/getting-started/processes.html) و [Task.async_stream](https://hexdocs.pm/elixir/Task.html)
- [Node.js event loop](https://nodejs.org/learn/asynchronous-work/event-loop-timers-and-nexttick) و [avoid blocking](https://nodejs.org/learn/asynchronous-work/dont-block-the-event-loop)

</div>
