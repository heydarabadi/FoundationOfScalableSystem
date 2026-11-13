<div dir="rtl" align="right">

# فصل 04: مروری بر سیستم‌های هم‌روند

![نمودار یادگیری و آزمایش فصل](04-overview-of-concurrent-systems-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

thread و زمان‌بندی؛ race و deadlock؛ هماهنگی؛ thread pool؛ barrier؛ collection امن برای thread.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 61–88.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

هم‌زمانی حتی روی یک CPU هم مفید است: وقتی کاری منتظر دیسک یا شبکه است، کار دیگر می‌تواند از پردازنده استفاده کند. سخت‌افزار چند‌هسته‌ای، اگر کارها مستقل باشند، اجرای واقعاً موازی را هم ممکن می‌کند. یکی از مدل‌ها thread است: هر thread stack خودش را دارد اما state سطح process را شریک می‌شود. scheduler با interleaving پیش‌دستانه و غیرقطعی کار می‌کند، پس ترتیب کد را نمی‌توان ترتیب اجرای تضمینی دانست. زبان‌ها مدل‌های دیگری هم دارند: channelهای CSP در Go، actorهای Erlang با پیام‌رسانی، و event loop در Node.js؛ هرکدام بده‌بستان متفاوتی دربارهٔ state مشترک و I/O دارند.

خطر اصلی صحت در state قابل‌تغییر مشترک race condition است: عملیاتی سطح‌بالا مثل increment ممکن است چند دستور ماشین باشد و interleaving باعث گم‌شدن update شود. invariant را در critical section کوتاه محافظت کن، اما lock کار را سری می‌کند و می‌تواند مقیاس‌پذیری را کم کند. lockها deadlock هم می‌سازند، اگر threadها منابع مشترک را با ترتیب متضاد بگیرند؛ ترتیب کلی و یکسان برای گرفتن lock چرخهٔ انتظار را می‌شکند، و timeout فقط راه‌حل جایگزین با رفتار تقریبی است. وضعیت threadها (created، runnable، blocked و terminated) را بشناس و برای هماهنگی از lifecycle signal مثل join استفاده کن، نه sleep حدسی.

کار producer-consumer به buffer محدود و هماهنگی نیاز دارد: polling CPU هدر می‌دهد، اما blocking queue اجازه می‌دهد producer برای جا و consumer برای پیام منتظر بماند. thread pool تعداد workerها را محدود و reuse می‌کند و taskهای اضافی را صف می‌کند تا thread و stack بی‌حساب ساخته نشوند؛ shutdown باید با قصد drain یا لغو کار انجام شود. barrier مثل CountDownLatch هماهنگ‌کننده را تا پایان کارهای موازی یک مرحله منتظر می‌گذارد. collection مشترک هم باید درست انتخاب شود: wrapper همگام امن است اما دسترسی را سری می‌کند؛ ساختار concurrent مثل ConcurrentHashMap به‌روزرسانی ریزدانه‌تر و تضمین snapshot/iteration ضعیف‌تری می‌دهد. قاعدهٔ عملی فصل این است که مدل هم‌روندی را آگاهانه انتخاب کنیم، state قابل‌تغییر مشترک را کم کنیم، critical section را کوتاه و pool و صف را محدود نگه داریم و هم صحت در زمان‌بندی‌های گوناگون و هم throughput و contention را بسنجیم.

### نکته‌های کلیدی

- هم‌زمانی هم‌پوشانی پیشرفت کارهاست؛ parallelism اجرای هم‌زمان است.
- invariantها را حفظ و state مشترک را کم کن.
- pool و صف را محدود کن؛ thread بیشتر ممکن است contention بسازد.

### پوشش موضوع‌های اصلی فصل

- چرایی هم‌زمانی، رفتار thread و اهمیت ترتیب اجرای غیرقطعی.
- race و deadlock؛ وضعیت thread و سازوکارهای هماهنگی.
- thread pool، barrier و collection ایمن برای کار موازی کنترل‌شده.

## درس آموزشی

### هدف‌های یادگیری و نقشهٔ مفهومی

- concurrency را از parallelism جدا و مدل اجرا را با workload تطبیق دهیم.
- race، deadlock و contention را با کنترل مالکیت و هماهنگی کاهش دهیم.
- worker و صف bounded طراحی کنیم تا overload به منابع نامحدود تبدیل نشود.

`ورودی → صف محدود → workerهای محدود → عملیات I/O یا CPU → نتیجه`

### توضیح مفاهیم

Concurrency یعنی چند کار در یک بازه پیشرفت کنند؛ parallelism یعنی چند کار روی چند هسته واقعاً هم‌زمان اجرا شوند. یک thread هنگام انتظار شبکه می‌تواند کار دیگری را انجام دهد، اما thread بیش از حد stack، context switch و contention را زیاد می‌کند. برای I/O-bound از async I/O یا workerهای محدود استفاده کن؛ برای CPU-bound موازی‌سازی را به ظرفیت هسته‌ها نزدیک کن و اندازه بگیر.

Threadها stack جدا و process state مشترک دارند. اگر دو thread متغیر مشترک را تغییر دهند، عملیاتی مثل `count++` ممکن است interleave شود و update گم شود. گاهی بهتر است به‌جای lock، state را مالکیت‌محور کنیم: هر worker مالک state خودش باشد و نتیجه‌ها ادغام شوند، یا actor پیام‌ها را به مالک یگانهٔ state برساند. اگر lock لازم است، invariant را کوتاه محافظت کن و ترتیب گرفتن چند lock را ثابت نگه دار. Deadlock زمانی شکل می‌گیرد که چرخهٔ انتظار منابع ساخته شود.

Producer-consumer به buffer محدود نیاز دارد. صف نامحدود وقتی مصرف‌کننده کند است فقط دیرتر حافظه را تمام می‌کند؛ صف bounded با wait/reject/drop امکان اعمال backpressure می‌دهد. Thread pool تعداد اجرای هم‌زمان را کنترل می‌کند. Shutdown هم باید بگوید کارهای پذیرفته‌شده drain می‌شوند یا cancellation می‌گیرند. Barrier برای منتظرماندن تا اتمام همهٔ workerهای یک مرحله است؛ `sleep` حدسی جای synchronization نیست.

### نمونهٔ C# با صف bounded

```csharp
var channel = Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(256)
{
    FullMode = BoundedChannelFullMode.Wait,
    SingleReader = false,
    SingleWriter = false
});

var workers = Enumerable.Range(0, workerCount).Select(_ => Task.Run(async () =>
{
    await foreach (var item in channel.Reader.ReadAllAsync(stoppingToken))
        await ProcessAsync(item, stoppingToken);
}));
```

`workerCount` و ظرفیت صف باید با latency، منابع و توان وابستگی تنظیم شوند، نه بر اساس عدد ثابت. برای shutdown، writer را کامل کن، مهلت drain بده و سپس cancellation را اعمال کن.

### انتخاب مدل و تمرین

| مدل | مناسب برای | هزینه/محدودیت |
| --- | --- | --- |
| Thread + lock | state مشترک کوچک با invariant کوتاه | contention و خطر deadlock |
| Async + bounded queue | I/O زیاد و نیاز به backpressure | async خودکار CPU parallelism نیست |
| Actor/mailbox | state با مالک یگانه و پیام‌رسانی | mailbox و supervision باید مدیریت شوند |
| CSP/channel | pipelineهای producer-consumer | blocking و head-of-line را بسنج |
| Event loop | I/O زیاد و عملیات غیرمسدودکننده | کار CPU سنگین loop را متوقف می‌کند |

- در critical section عملیات شبکه یا DB انجام نده و صف/connection pool را بی‌سقف نگذار.
- cancellation را تا پایین‌ترین API قابل لغو منتقل کن.
- تست کن دو worker برای یک key نتوانند state ناسازگار بسازند؛ هم‌زمان contention و tail latency را اندازه بگیر.

### چرخهٔ عمر و هماهنگی threadها

Thread معمولاً از حالت ایجادشده به runnable می‌رود؛ scheduler آن را اجرا می‌کند. هنگام انتظار برای lock یا منبع در حالت blocked است، برای رخداد/شرط منتظر می‌ماند، ممکن است timed-wait داشته باشد و پس از پایان terminated می‌شود. این وضعیت‌ها برای تشخیص threadی که CPU می‌گیرد با threadی که فقط منتظر است مهم‌اند؛ تعداد thread زیاد به معنی parallelism مفید نیست.

Lock بخش بحرانی را از race محافظت می‌کند، اما ترتیب گرفتن چند lock باید ثابت باشد تا چرخهٔ deadlock ساخته نشود. Semaphore تعداد دسترسی هم‌زمان به منبع محدود را کنترل می‌کند؛ condition/barrier هماهنگی مرحله‌ای می‌سازد. Barrier همهٔ شرکت‌کنندگان را تا رسیدن آخرین عضو نگه می‌دارد، بنابراین یک عضو کند می‌تواند کل مرحله را معطل کند. Thread pool هزینهٔ ساخت thread را کم می‌کند ولی باید با ظرفیت I/O/CPU تنظیم شود. Concurrent collection عملیات مشخص را امن می‌کند، اما چند عملیات پیاپی را خودبه‌خود transaction نمی‌کند.

### خودآزمایی همراه پاسخ

1. **پرسش:** race condition و deadlock چه تفاوتی دارند؟
   **پاسخ کوتاه:** Race یعنی نتیجه به interleaving وابسته است؛ deadlock یعنی کارها در انتظار قفل/منبع همدیگر چرخه می‌سازند و پیشروی نمی‌کنند.

2. **پرسش:** چرا صف bounded برای workerها مهم است؟
   **پاسخ کوتاه:** ظرفیت بافر را محدود می‌کند تا ورودی سریع‌تر از پردازش باعث رشد نامحدود حافظه نشود؛ پرشدن صف باید backpressure یا رد کنترل‌شده بسازد.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `ConcurrentExecutionLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter04
- **پوشهٔ مستقل راه‌حل:** solutions/04-overview-of-concurrent-systems/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Implement the same bounded work queue with fixed concurrency levels and cancellation; verify ordering and shared-state safety.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

Throughput, queue wait, allocation, CPU, lock contention, and tail latency as concurrency changes.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## منابع

### مبنای کتاب

- Ian Gorton، *Foundations of Scalable Systems*، فصل 04: “An Overview of Concurrent Systems” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch04.html).

### مستندات فنی بیرونی

- مستندات فنی اولیه: [مرجع رسمی](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/).

### پیوندهای این مخزن

- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
