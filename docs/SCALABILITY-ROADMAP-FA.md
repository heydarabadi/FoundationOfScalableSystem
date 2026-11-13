<div dir="rtl" align="right">

# نقشهٔ یادگیری و راه‌حل‌های سیستم مقیاس‌پذیر

این مخزن مسیر یادگیری خود را بر اساس کتاب *Foundations of Scalable Systems* نوشتهٔ Ian Gorton تنظیم می‌کند. کتاب ترتیب مفاهیم را می‌دهد و راه‌حل‌های مخزن، پیاده‌سازی‌های مستقل برای آزمایش آن مفاهیم هستند. این سند همراه آموزشی و نقشهٔ راه است، نه بازنویسی کتاب. عنوان فصل‌ها در جدول‌ها به‌اختصار و با بیان خودمان آمده‌اند.

**منبع اصلی:** [O’Reilly — Foundations of Scalable Systems](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/)

## روش استفاده از مخزن

۱. یک اصل طراحی را از کتاب یاد بگیریم.
۲. یک راه‌حل مستقل را انتخاب کنیم که آن اصل را ملموس می‌کند.
۳. workload یکسان را اجرا و throughput، latency دنباله‌ای، خطاها، منابع، دوام داده و هزینهٔ عملیاتی را مقایسه کنیم.
۴. نتیجه را همراه سخت‌افزار و تنظیمات ثبت کنیم. اسم فناوری یا عدد هدف، به‌تنهایی اثبات ظرفیت نیست.

هر رویکرد در پوشهٔ مستقل `solutions/<solution-name>/` قرار می‌گیرد و کد، وابستگی‌ها، زیرساخت، workload و README خودش را دارد. برای ایده‌هایی که هنوز پیاده‌سازی نشده‌اند پوشهٔ خالی نمی‌سازیم. پروفایل‌های Lab و Performance در راه‌حل فصل ۱۴، [Scalable Event-Driven Processing](../solutions/14-scalable-event-driven-processing/README.fa.md)، همچنان یک پیاده‌سازی هستند.

## راهنماهای هر فصل

برای مقایسهٔ چند معماری کامل برای مسئلهٔ سبد و سفارش، از [طرح‌های قابل پیاده‌سازی فارسی](IMPLEMENTABLE-SOLUTION-BLUEPRINTS-FA.md) یا [نسخهٔ انگلیسی](IMPLEMENTABLE-SOLUTION-BLUEPRINTS-EN.md) شروع کن. این‌ها الگوهای طراحی‌اند؛ فقط راه‌حل فصل ۱۴ در حال حاضر پیاده‌سازی اجرایی دارد.

برای مطالعهٔ عمیق‌تر خانواده‌های داده، [راهنمای پیاده‌سازی پایگاه‌داده‌ها](DATABASE-FAMILIES-IMPLEMENTATION-GUIDE-FA.md) را بخوان؛ برای مقایسهٔ concurrency و زبان‌ها، [راهنمای زبان‌ها و مدل‌های هم‌روندی](CONCURRENCY-LANGUAGES-AND-APPROACHES-FA.md) را ببین. [راهنمای رویکردهای معماری](ARCHITECTURAL-APPROACHES-PLAYBOOK-FA.md) هم monolith، sync API، queue، Kafka، outbox، CQRS، Event Sourcing، Saga، stream processing، cache و serverless را کنار هم مقایسه می‌کند. این راهنماها خانواده‌های relational، key-value، document، wide-column، distributed SQL، graph، search و analytical storage را با workload و محدودیت بررسی می‌کنند و برای .NET، Go، Rust، JVM/Scala، Elixir و Node.js مسیر benchmark تعریف می‌کنند.

برای هر فصل README دوزبانه و یک تصویر مفهومی SVG اختصاصی وجود دارد. فصل‌هایی که هنوز کد اجرایی ندارند صریحاً فقط راهنما و طرح هستند؛ نام پروژه و namespace آن‌ها تا زمان پیاده‌سازی پیشنهادی است.

| فصل | راهنما |
|---|---|
| 01. مقدمه‌ای بر سیستم‌های مقیاس‌پذیر | [فارسی](../solutions/01-introduction-to-scalable-systems/README.fa.md) · [English](../solutions/01-introduction-to-scalable-systems/README.md) |
| 02. آشنایی با معماری سیستم‌های توزیع‌شده | [فارسی](../solutions/02-distributed-systems-architectures/README.fa.md) · [English](../solutions/02-distributed-systems-architectures/README.md) |
| 03. مبانی سیستم‌های توزیع‌شده | [فارسی](../solutions/03-distributed-systems-essentials/README.fa.md) · [English](../solutions/03-distributed-systems-essentials/README.md) |
| 04. مروری بر سیستم‌های هم‌روند | [فارسی](../solutions/04-overview-of-concurrent-systems/README.fa.md) · [English](../solutions/04-overview-of-concurrent-systems/README.md) |
| 05. سرویس‌های برنامه | [فارسی](../solutions/05-application-services/README.fa.md) · [English](../solutions/05-application-services/README.md) |
| 06. Caching توزیع‌شده | [فارسی](../solutions/06-distributed-caching/README.fa.md) · [English](../solutions/06-distributed-caching/README.md) |
| 07. پیام‌رسانی ناهمگام | [فارسی](../solutions/07-asynchronous-messaging/README.fa.md) · [English](../solutions/07-asynchronous-messaging/README.md) |
| 08. سامانه‌های پردازش Serverless | [فارسی](../solutions/08-serverless-processing-systems/README.fa.md) · [English](../solutions/08-serverless-processing-systems/README.md) |
| 09. Microservices | [فارسی](../solutions/09-microservices/README.fa.md) · [English](../solutions/09-microservices/README.md) |
| 10. مبانی پایگاه‌دادهٔ مقیاس‌پذیر | [فارسی](../solutions/10-scalable-database-fundamentals/README.fa.md) · [English](../solutions/10-scalable-database-fundamentals/README.md) |
| 11. سازگاری نهایی | [فارسی](../solutions/11-eventual-consistency/README.fa.md) · [English](../solutions/11-eventual-consistency/README.md) |
| 12. سازگاری قوی | [فارسی](../solutions/12-strong-consistency/README.fa.md) · [English](../solutions/12-strong-consistency/README.md) |
| 13. پیاده‌سازی‌های پایگاه‌دادهٔ توزیع‌شده | [فارسی](../solutions/13-distributed-database-implementations/README.fa.md) · [English](../solutions/13-distributed-database-implementations/README.md) |
| 14. Scalable Event-Driven Processing — پردازش رویدادمحور مقیاس‌پذیر | [درس کامل فارسی](../solutions/14-scalable-event-driven-processing/CHAPTER-GUIDE-FA.md) · [راهنمای اجرای فارسی](../solutions/14-scalable-event-driven-processing/README.fa.md) · [English](../solutions/14-scalable-event-driven-processing/README.md) |
| 15. سامانه‌های پردازش Stream | [فارسی](../solutions/15-stream-processing-systems/README.fa.md) · [English](../solutions/15-stream-processing-systems/README.md) |
| 16. توصیه‌های پایانی برای موفقیت | [فارسی](../solutions/16-final-tips-for-success/README.fa.md) · [English](../solutions/16-final-tips-for-success/README.md) |
| فصل کتاب | موضوع به‌اختصار | کاربرد در مخزن |
|---|---|---|
| ۱. مقدمهٔ سیستم‌های مقیاس‌پذیر | مقیاس‌پذیری، هزینه و مصالحه‌های معماری | پیش از انتخاب طراحی، workload و SLO را تعریف کنیم و ظرفیت را کنار هزینه و بهره‌برداری بسنجیم. |
| ۲. معماری سیستم‌های توزیع‌شده | scale-out، cache، توزیع داده و لایه‌های پردازش | راه‌حل فعلی نمونهٔ چندلایه است؛ یک مسیر synchronous ساده هم برای مقایسه اضافه کنیم. |
| ۳. مبانی سیستم‌های توزیع‌شده | ارتباط، شکست جزئی، consensus و زمان | برای هر راه‌حل timeout، retry، شکست، و تحویل تکراری را مستند و آزمایش کنیم. |
| ۴. سیستم‌های هم‌روند | thread، race، deadlock، هماهنگی و pool | سقف concurrency و backpressure را در پیاده‌سازی زبان‌های مختلف مقایسه کنیم. |
| ۵. سرویس‌های برنامه | API، state، مقیاس افقی، توزیع بار و سلامت | API بدون state محلی را نگه داریم و scale و توزیع ترافیک را اندازه بگیریم. |
| ۶. cache توزیع‌شده | cache برنامه و وب، cache HTTP | حالت بدون cache را با projection مبتنی بر Redis بسنجیم و انقضا و دادهٔ قدیمی را مشخص کنیم. |
| ۷. پیام‌رسانی ناهمگام | ماندگاری پیام، pub/sub، competing consumers و ایمنی | Kafka موجود را گسترش دهیم: retry، poison message، idempotency و outbox را جداگانه پوشش دهیم. |
| ۸. پردازش serverless | پردازش رویدادمحور، چرخهٔ اجرا، autoscaling و هزینه | گزینهٔ بعدی: یک مسیر managed function/queue را با worker همیشه‌فعال مقایسه کنیم. |
| ۹. Microservices | شکستن سیستم، تاب‌آوری، cascading failure و bulkhead | API و processorهای فعلی را نمونهٔ مرزبندی سرویس بگیریم و جداسازی شکست را بسنجیم. |
| ۱۰. مبانی پایگاه‌دادهٔ مقیاس‌پذیر | scale-up/out رابطه‌ای، replica، partitioning، NoSQL و CAP | PostgreSQL را با read replica/partitioning و چند مدل NoSQL منتخب مقایسه کنیم. |
| ۱۱. سازگاری نهایی | quorum، repair، تعارض و read-your-writes | تضمین consistency را بنویسیم و stale read، بازیابی و conflict را بررسی کنیم. |
| ۱۲. سازگاری قوی | تراکنش توزیع‌شده، 2PC، consensus و Raft | تراکنش رابطه‌ای را در صورت نیاز با یک گزینهٔ distributed SQL مقایسه کنیم. |
| ۱۳. پیاده‌سازی پایگاه‌داده‌های توزیع‌شده | مدل‌ها و trade-offهای Redis، MongoDB و DynamoDB | فقط برای مدل دادهٔ مناسب، سناریوی هم‌ارز بسازیم و دوام و semantics پرس‌وجو را مقایسه کنیم. |
| ۱۴. پردازش رویدادمحور مقیاس‌پذیر | Kafka، topic، producer/consumer، مقیاس و دسترس‌پذیری | راه‌حل فعلی؛ partition، کلید، lag، replay و سقف consumer را روشن کنیم. |
| ۱۵. پردازش stream | window، پلتفرم stream، Flink و ایمنی داده | بعداً برای join یا aggregation زمان‌مند، stream processing واقعی اضافه کنیم. |
| ۱۶. نکته‌های پایانی | automation، observability، deployment و data lake | برای هر راه‌حل، استقرار تکرارپذیر، telemetry، بار، پاک‌سازی و ثبت نتیجه لازم است. |

## مسیرهای پیشنهادی راه‌حل

این‌ها پیشنهاد هستند؛ به معنی پیاده‌سازی فعلی یا برتری قطعی یک فناوری نیستند. ابتدا یک workload را ثابت نگه می‌داریم و هر بار فقط یک انتخاب بزرگ را تغییر می‌دهیم.

| مسیر | چه چیزی را جداگانه می‌سنجد | فناوری‌های پیشنهادی | وضعیت پیشنهادی |
|---|---|---|---|
| مبنای event-driven | صف پایدار، کار partition‌شده و scale مصرف‌کننده | راه‌حل فعلی: .NET، Kafka، Redis، PostgreSQL و Kubernetes | پیاده‌سازی شده؛ پیش از ادعای ظرفیت، هر سناریو باید اندازه‌گیری شود. |
| مبنای synchronous و رابطه‌ای | درخواست/تراکنش مستقیم در برابر صف | .NET یا Go با PostgreSQL؛ در صورت نیاز MySQL به‌عنوان variant مستقل | گام بعد: workload هم‌معنا با تضمین تراکنش یکسان. |
| گونه‌های مقیاس پایگاه‌داده | read scaling و تفاوت مدل رابطه‌ای، سندی و کلید-مقدار | PostgreSQL replica/partitioning، MongoDB، DynamoDB و Redis برای cache/projection | فقط با قرارداد consistency و durability مشخص اضافه شوند. |
| CQRS و transactional outbox | جداسازی مدل خواندن/نوشتن و انتشار مطمئن پیام از DB | .NET یا Go، PostgreSQL و Kafka | پس از baseline؛ هزینهٔ write و تأخیر projection سنجیده شود. |
| Event sourcing | تاریخچهٔ پایدار رویداد، replay و snapshot | Kafka همراه event store یا پایگاه‌دادهٔ پایدار؛ زبان بر اساس variant | راه‌حل مستقل؛ صرف retention در Kafka را بدون تضمین روشن، event store کامل فرض نکنیم. |
| پردازش stream | window، join، aggregation و بازیابی state | Kafka و Flink در اکوسیستم Java/Scala؛ در جای مناسب Kafka Streams | بعداً، برای workloadی که واقعاً پردازش زمان‌مند/stateful می‌خواهد. |
| مقایسهٔ زبان و runtime | هزینهٔ runtime، حافظه، concurrency و latency | C#/.NET، Go، Rust، Scala/JVM و Elixir/BEAM | با پروتکل، مدل داده، وابستگی و بار هم‌ارز؛ نتایج همراه جزئیات اجرا. |

## راهنمای انتخاب پایگاه‌داده

- **PostgreSQL** مرجع فعلی تراکنش checkout و inventory است. ابتدا query، index، اتصال و مرز تراکنش را بهینه کنیم؛ سپس اگر اندازه‌گیری bottleneck خواندن یا حجم داده را نشان داد، replica یا partitioning را بررسی کنیم.
- **MySQL** می‌تواند مقایسهٔ رابطه‌ای باشد؛ اما schema intent و semantics تراکنش باید هم‌ارز باشد و نتیجه با تعویض خاموش DB مخلوط نشود.
- **Redis** برای cache و projection سریع در طراحی فعلی مناسب است. روشن کنیم مقدارها دورریختنی‌اند، قابل بازسازی‌اند یا باید پایدار بمانند؛ eviction و recovery را هم بسنجیم.
- **MongoDB** برای دسترسی سندمحور قابل بررسی است؛ query، هزینهٔ index، کلید توزیع و consistency را بسنجیم، نه این‌که آن را جایگزین مستقیم SQL فرض کنیم.
- **DynamoDB** گزینهٔ مدیریت‌شدهٔ key-value/document بر پایهٔ partition key و عملیات شرطی است. هزینهٔ Cloud، محدودیت‌ها و رفتار چندمنطقه‌ای باید در نتیجه دیده شود.
- **Distributed SQL** مثل CockroachDB یا YugabyteDB وقتی قابل بررسی است که هم SQL transaction و هم توزیع افقی نیاز باشند؛ latency تراکنش، contention و هزینهٔ عملیات باید آزموده شوند.

مقایسهٔ پایگاه‌داده منصفانه نیست مگر یک عملیات کسب‌وکار قابل‌مشاهده را با قرارداد consistency، durability و failure یکسان اجرا کند.

## قواعد مقایسهٔ زبان‌های برنامه‌نویسی

هدف، بررسی high-performance programming است؛ از قبل برنده‌ای تعیین نمی‌کنیم.

- **C#/.NET:** پیاده‌سازی مرجع فعلی؛ async I/O، allocation، فشار thread pool و bottleneck کلاینت/DB را اندازه بگیریم.
- **Go:** گزینه‌ای برای سرویس یا consumer هم‌روند؛ زمان‌بندی goroutine، allocation و backpressure را بسنجیم.
- **Rust:** گزینه‌ای وقتی هزینهٔ CPU، کنترل حافظه یا tail latency قابل‌پیش‌بینی مهم است؛ پیچیدگی توسعه و عملیات هم باید در مقایسه بیاید.
- **Scala/JVM:** گزینه‌ای برای پردازش stream و pipelineهای Kafka؛ warm-up، GC و رفتار steady-state را ثبت کنیم.
- **Elixir/BEAM:** گزینه‌ای برای تعداد زیاد processهای هم‌روند و جداسازی خطا در workload پیام‌محور. برای بار CPU-heavy فرض نکنیم سریع‌تر است؛ workload مناسب خودش را benchmark کنیم.

برای مقایسهٔ معتبر، نسخهٔ runtime/client، schema و partition key، semantics retry/commit و warm-up هم‌ارز باشند. p50/p95/p99، throughput، خطا، CPU، حافظه و بازیابی را همراه سخت‌افزار و تنظیمات ثبت کنیم. اگر هر زبان پیاده‌سازی کامل خودش را دارد، در پوشهٔ solution مستقل بماند؛ فقط تعریف benchmark مشترک می‌تواند در اسناد ریشه باشد.

## ترتیب پیشنهادی پیاده‌سازی

۱. راه‌حل Kafka فعلی را مثال اول نگه داریم و baseline تکرارپذیر ثبت کنیم.
۲. یک راه‌حل synchronous با PostgreSQL به‌عنوان کنترل اضافه کنیم؛ workload سبد/سفارش و تضمین تراکنش روشن باشد.
۳. یک variant مقیاس DB، مثل replica خواندن یا partitioning، بر اساس bottleneck اندازه‌گیری‌شده بسازیم.
۴. CQRS/outbox و event sourcing را به‌عنوان طراحی‌های مستقل با قرارداد consistency، replay و duplicate-handling اضافه کنیم.
۵. وقتی workload به window، join یا event-time نیاز داشت، مثال پردازش stateful stream بسازیم.
۶. یک مسیر داغ را به Go یا Rust منتقل کنیم؛ بعد Scala یا Elixir را جایی بررسی کنیم که مدل هم‌روندی یا پردازش‌شان پرسش مشخصی را جواب دهد.

هر solution تازه باید README محلی، روش اجرا و پاک‌سازی، workload، توضیح معماری، محدودیت‌ها و اندازه‌گیری یا وضعیت روشن «اندازه‌گیری نشده» داشته باشد. README ریشه فهرست بماند و هر راه‌حل به این نقشهٔ راه وصل شود.

</div>
