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

## ۴. نمونهٔ worker محدود در زبان‌ها

### C# — Channel با backpressure

```csharp
var queue = Channel.CreateBounded<Command>(new BoundedChannelOptions(512)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = false,
    SingleWriter = false
});

var workers = Enumerable.Range(0, concurrency)
    .Select(_ => ConsumeAsync(queue.Reader, stoppingToken));
```

### Go — worker pool و channel با ظرفیت

```go
jobs := make(chan Job, 512)
for i := 0; i < workerCount; i++ {
    go func() {
        for job := range jobs {
            process(job)
        }
    }()
}
```

ارسال به channel پر منتظر می‌ماند؛ این همان backpressure است. بستن channel، cancellation، error propagation و اطمینان از خروج goroutineها باید جزو طراحی باشند.

### Elixir — concurrency محدود

```elixir
items
|> Task.async_stream(&process/1,
  max_concurrency: worker_count,
  ordered: false,
  timeout: 5_000
)
|> Stream.run()
```

`max_concurrency` را به توان دیتابیس و سرویس خارجی محدود کن. mailbox یا task نامحدود می‌تواند حافظه را رشد دهد؛ process سبک به معنی ظرفیت نامتناهی نیست.

### Rust / Java / Scala

برای هر زبان، معادل bounded queue/limiter را با library پایدار همان runtime انتخاب کن؛ queue یا task نامحدود نساز. در Rust، نوع‌ها و ownership بسیاری از raceهای حافظه‌ای را در compile-time محدود می‌کنند اما logical race و deadlock هنوز ممکن‌اند. در Java/Scala، `ExecutorService`/Futures یا virtual threads را با semaphore برای منابع محدود ترکیب کن؛ virtual threadهای فراوان، ظرفیت دیتابیس را زیاد نمی‌کنند.

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

- [.NET async](https://learn.microsoft.com/en-us/dotnet/standard/async-in-depth) و [Channels/backpressure](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)
- [Go Effective Go: concurrency](https://go.dev/doc/effective_go#concurrency)
- [Rust: Fearless Concurrency](https://doc.rust-lang.org/book/ch16-00-concurrency.html)
- [Java virtual threads](https://docs.oracle.com/en/java/javase/21/core/virtual-threads.html)
- [Scala Futures](https://docs.scala-lang.org/overviews/core/futures.html)
- [Elixir processes](https://elixir-lang.org/getting-started/processes.html) و [Task.async_stream](https://hexdocs.pm/elixir/Task.html)
- [Node.js event loop](https://nodejs.org/learn/asynchronous-work/event-loop-timers-and-nexttick) و [avoid blocking](https://nodejs.org/learn/asynchronous-work/dont-block-the-event-loop)

</div>
