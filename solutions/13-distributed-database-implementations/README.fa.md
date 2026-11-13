<div dir="rtl" align="right">

# فصل 13: پیاده‌سازی‌های پایگاه‌دادهٔ توزیع‌شده

![نمودار یادگیری و آزمایش فصل](13-distributed-database-implementations-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

مدل داده و API؛ توزیع و replication؛ نقاط قوت و محدودیت Redis، MongoDB و DynamoDB.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 247–268.** این جمع‌بندی با بیان خودمان نوشته شده است؛ برای شرح کامل به کتاب مراجعه کن. نسخهٔ محصول و محدودیت سرویس مدیریت‌شده تغییر می‌کند؛ پیش از پیاده‌سازی مستند فعلی فروشنده را بررسی کن.

فصل Redis، MongoDB و DynamoDB را از روی پیاده‌سازی واقعی مقایسه می‌کند، نه صرفاً برچسب محصول. قابلیت‌های هم‌نام ممکن است رفتار متفاوت داشته باشند؛ پس مدل داده، دوام، partitioning، replication، consistency، بازیابی خرابی و هزینه‌ای را که برنامه به آن تکیه می‌کند دقیق بسنج.

Redis یک store درون‌حافظه‌ای key-value و data-structure با primitiveهای string، list، set/sorted-set و hash است. عملیات روی یک key اتمی است؛ MULTI/EXEC گروهی از فرمان‌ها را سری می‌کند، اما rollback ندارد و transaction کامل ACID نیست. snapshot و append-only file (AOF) بازیابی را بهتر می‌کنند ولی هزینهٔ write/latency دارند. Redis Cluster کلیدها را با hash slot توزیع می‌کند؛ عملیات چندکلیدی باید در یک slot باشند و hash tag می‌تواند این هم‌مکانی را بسازد. replication ناهمگام است؛ WAIT برای تأیید replicaها صبر می‌کند ولی دوام روی دیسک replica را تضمین نمی‌کند. هنگام failover ممکن است replicaای رهبر شود که همهٔ writeها را نگرفته؛ پس از دست رفتن داده ممکن است. Redis برای cache کم‌تأخیر، counter، session و state سریعِ درست انتخاب‌شده مناسب است؛ فصل هشدار می‌دهد بدون سنجش تضمین persistence و failover، تنها منبع دادهٔ غیرقابل‌جایگزینش نکن.

MongoDB اسناد BSON را در collection نگه می‌دارد، nested data و index دارد و write روی یک سند اتمی است. denormalize کردن داده‌های مرتبط در یک سند می‌تواند هماهنگی چندسندی را حذف کند؛ transaction برای چند سند ممکن است ولی هزینه و جزئیات isolation دارد. sharding با shard key از نوع hash یا range و route از مسیر mongos انجام می‌شود؛ key نامناسب skew/hot spot می‌سازد و query بین shardها پرهزینه‌تر است. replica set از primary/oplog و secondary استفاده می‌کند و election پس از خرابی رهبر را بازمی‌گرداند. write concern میزان تأیید دوام را کنترل می‌کند؛ read preference تازگی را با نزدیکی/ظرفیت مبادله می‌کند. این گزینه برای دادهٔ کسب‌وکار سندمحور و در حال تکامل انعطاف دارد، به شرط آن‌که تکامل schema، shard key و consistency آگاهانه طراحی شوند.

DynamoDB سرویس مدیریت‌شدهٔ key-value/document است که بر partition key، sort key اختیاری، secondary index و محدودیت اندازهٔ آیتم تکیه دارد. الگوی دسترسی باید طراحی key/index را تعیین کند؛ batch رفت‌وبرگشت شبکه را کم می‌کند و transaction بین چند آیتم هماهنگی و هزینهٔ capacity دارد. سرویس محل shard و replication را پنهان می‌کند؛ read پیش‌فرض ممکن است eventual باشد، در حالی‌که strong read هزینهٔ بیشتر دارد و منطقه‌ای است. global table چندمنطقه‌ای را ناهمگام replicate می‌کند و ممکن است تعارض همزمان را با last-writer-wins حل کند؛ بنابراین تعارض بین منطقه‌ها و دامنهٔ transaction باید دیده شود. adaptive capacity برای بار متوازن کمک می‌کند، ولی hot key همچنان می‌تواند سقف throughput یا throttling بسازد. یکپارچگی با AWS و مدیریت‌شدن مزیت‌اند؛ وابستگی به vendor، مدل هزینه و طراحی key از نگرانی‌هاست.

خروجی درست، مقایسهٔ آزمایشی برای یک مورد استفاده است، نه رتبه‌بندی همیشگی. عملیات و قرارداد صحت معادل را روی هر گزینه بسنج و رفتار عادی و شکست را اندازه بگیر. وقتی کسب‌وکار منبع حقیقت بادوام می‌خواهد، دیتابیس تراکنشی را authoritative نگه دار و Redis یا projection سندی را فقط برای access pattern سریع‌تر با مسیر sync/rebuild مشخص اضافه کن.

### نکته‌های کلیدی

- پیش از مقایسه، عملیات را به مدل بومی هر دیتابیس نگاشت کن.
- persistence، replication، recovery و پیچیدگی عملیات را بسنج.
- workload و semantics معادل پیش‌شرط benchmark مفید است.

### پوشش موضوع‌های اصلی فصل

- ساختارهای Redis، snapshot/AOF، hash slot، محدودیت چندکلیدی در یک slot، replication ناهمگام و مصالحهٔ WAIT/failover.
- سندهای BSON در MongoDB، اتمی بودن update سند، transaction چندسندی، shard key/mongos/مهاجرت chunk، replica set، write concern و read preference.
- partition/sort key، secondary index، batch/transaction، replication مدیریت‌شده، read قوی/نهایی، global table و رفتار hot-key/capacity در DynamoDB.
- due diligence مقایسه‌ای: تضمین واقعی، workload، بازیابی و هزینه را آزمایش کن؛ نام قابلیت مشابه را معادل فرض نکن.

## درس آموزشی: مقایسهٔ موتور واقعی

### هدف‌های یادگیری و نقشهٔ مفهومی

- از قابلیت محصول به semantics واقعی read/write/failure برسیم.
- مدل داده و partition key را برای queryهای برنامه بسازیم.
- durability، availability، replication و persistence را از هم جدا کنیم.

`business invariant + access pattern → data model → routing/partitioning → consistency/durability → failure drill`

### Redis

Redis ساختارهای دادهٔ in-memory دارد و برای lookup سریع، cache و state کلیدمحور مناسب است. ابتدا تصمیم بگیر داده disposable است، از منبع دیگری rebuild می‌شود یا حقیقت پایدار است. RDB snapshot و AOF تغییرات logشده را ذخیره می‌کنند و durability/latency متفاوت دارند؛ replication برای HA است ولی ممکن است async باشد و با persistence یکی نیست. Cluster hash slotها را توزیع می‌کند؛ عملیات چندکلیدی ممکن است نیاز داشته باشد keyها هم‌slot باشند. برای payment/inventory، failover و از دست‌رفتن آخرین write را با نیاز دامنه مقایسه کن.

### MongoDB

مدل سند برای aggregateهایی است که معمولاً با هم خوانده یا تغییر می‌کنند. یک سند می‌تواند atomic باشد؛ embedding/denormalization را بر اساس access pattern انتخاب کن. Multi-document transaction وجود دارد اما هزینه دارد و جای schema مناسب را نمی‌گیرد. Replica set برای دسترس‌پذیری و انتخاب primary است؛ sharding نیازمند shard key با توزیع و routing مناسب است. Read/write concern را در آزمون و failure drill صریح کن.

### DynamoDB

دسترسی را با partition key و sort key مدل کن؛ queryهای اصلی باید از پیش معلوم باشند. key کم‌تنوع یا hot ممکن است بار را روی بخشی از ظرفیت متمرکز کند. Conditional update و transaction چند آیتمی برای workflow محدود ممکن است، اما هزینه و محدودیت سرویس را بررسی کن. سرویس مدیریت‌شده بخشی از ادارهٔ node را حذف می‌کند، نه مدل‌سازی access pattern را.

### مقایسه و تمرین

| عملیات | PostgreSQL | Redis | MongoDB | DynamoDB |
| --- | --- | --- | --- | --- |
| رزرو چند SKU | transaction مناسب | نیازمند پروتکل سازگاری جدا | transaction چند سند با هزینه | conditional items/transaction با محدودیت سرویس |
| cache سبد | شدنی، معمولاً منبع اصلی نباشد | طبیعی و کم‌تأخیر | document read model | key-based projection |
| گزارش joinمحور | SQL قوی | نامناسب برای query آزاد | aggregation با trade-off | access-pattern محدود |

سه variant برای «افزودن کالا به سبد» بساز. هرکدام idempotency key، version/order، expiry، read-your-write و recovery را مشخص کند. با payload و ترکیب بار یکسان و قطع node، correctness، p99، durability و هزینه را مقایسه کن.

### خواندن پیاده‌سازی‌ها به شکل مقایسه‌ای

در Redis، مدل commandهای key/value و ساختارهایی مثل hash، set و sorted set معمولاً برای دسترسی سریع به کلید مناسب‌اند؛ persistence و replication باید از configuration واقعی همان نسخه بررسی شوند. MongoDB سند را برای خواندن/نوشتن aggregateهای مرتبط کنار هم می‌گذارد؛ index، اندازهٔ سند، shard key و انتخاب replica read اهمیت دارند. DynamoDB access pattern را با partition key/sort key و indexهای از پیش طراحی‌شده جواب می‌دهد؛ کلید داغ، throttling و هزینهٔ ظرفیت باید سنجیده شود.

برای یک مدل مقایسهٔ درست، عملیات یکسان (مثلاً get/update سبد با version شرطی) و دادهٔ هم‌حجم را بساز؛ سپس throughput، p99، write amplification، durability پس از crash، failover، هزینه و complexity را اندازه بگیر. محصول به‌تنهایی تضمین consistency یا durability ثابتی ندارد؛ نسخه، topology و تنظیمات بخشی از نتیجه‌اند.

### مثال عملی: یک سبد در سه مدل داده

برای `basketId=42`، در Redis می‌توان کلید `basket:42` را به hash سبد تبدیل کرد؛ در MongoDB یک سند `{ _id: 42, version: 8, items: [...] }` کل aggregate را نگه دارد؛ در DynamoDB کلید اصلی می‌تواند `PK=BASKET#42` و `SK=ITEM#<sku>` باشد و metadata سبد در sort key جدا ذخیره شود. در هر سه حالت update شرطی روی version یا stock لازم است تا دو update هم‌زمان بی‌صدا همدیگر را overwrite نکنند.

برای مقایسه، همان فرمان «افزودن SKU با idempotency key» را اجرا کن. در Redis، رفتار transaction/script و persistence را؛ در MongoDB، atomic update روی سند و index را؛ در DynamoDB، conditional write و طراحی query را بررسی کن. اگر یک سبد واقعاً از چند سند/کلید تشکیل شده، مرز transaction هر گزینه و rebuild/backup را هم اندازه بگیر. این مثال شکل مدل‌سازی است؛ syntax و جزئیات اجرایی را با نسخه و تنظیم واقعی سرویس بررسی کن.

### خودآزمایی همراه پاسخ

1. **پرسش:** Redis، MongoDB و DynamoDB را با چه مبنایی مقایسه می‌کنیم؟
   **پاسخ کوتاه:** با access pattern، مدل داده و query، durability/replication، consistency، هزینهٔ عملیات و failure behavior؛ نه با برچسب کلی NoSQL.

2. **پرسش:** چرا تست default configuration کافی نیست؟
   **پاسخ کوتاه:** چون persistence، replication، acknowledgment و consistency تنظیم‌پذیرند؛ باید هر موتور را با تضمین‌های هدف هم‌ارز پیکربندی و در failover آزمایش کرد.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `DistributedDatabaseModelComparisonLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter13
- **پوشهٔ مستقل راه‌حل:** solutions/13-distributed-database-implementations/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

یک workload کاتالوگ محصول و سبد کاربر را با PostgreSQL (منبع حقیقت)، Redis (سبد/session/cache داغ)، MongoDB (کاتالوگ سندی) و DynamoDB (مدل access pattern کلیدمحور) بساز. یک query را به زور روی هر دیتابیسی اجرا نکن اگر با مدل بومی آن جور نیست؛ مشخص کن کدام نسخه projection است نه رکورد authoritative.

۱. رکوردهای canonical، عملیات read/write، تازگی لازم، معنای شکست و داده‌ای را که از دست‌دادنش پذیرفتنی نیست تعریف کن.
۲. key/index/shard key بومی هر گزینه را از مجموعهٔ queryها طراحی کن؛ حالت hot key و tenantهای نامتوازن را هم اضافه کن.
۳. رفتار API و dataset قطعی یکسان باشد؛ کپی cache/document/table را projection علامت بزن و روش rebuild/reconcile را تعریف کن.
۴. restart، failover primary، read کهنه از replica، hot key و update همزمان را تست کن. مشخص کن با تنظیم فعلی کدام عملیات ممکن است stale ببینند یا write تأییدشده را از دست بدهند.
۵. latency، throughput، هزینهٔ زیرساخت، کار backup/recovery و پیچیدگی مهندسی را مقایسه کن؛ از یک اجرای بی‌خطا نتیجهٔ کلی نگیر.

### انتخاب‌های راه‌حل قابل پیاده‌سازی

| راه‌حل | چه زمانی انتخابش کنیم | شکل اجرایی | هزینه‌ها و محدودیت‌ها |
|---|---|---|---|
| PostgreSQL + Redis cache | منبع تراکنشی قوی لازم است ولی read تکراری و داغ داریم | PostgreSQL منبع حقیقت → Redis به‌شکل cache-aside با کلید entity؛ رویداد version/invalidation؛ TTL و fallback روی cache miss | invalidation، جلوگیری از stampede و warmup؛ Redis نباید بی‌صدا تنها کپی بادوام شود |
| سرویس سندی MongoDB | read معمولاً یک aggregate تو‌در‌تو و منسجم را می‌خواهد که با هم تغییر می‌کند | .NET API → یک سند BSON برای catalog item/aggregate؛ index برای filterهای لازم؛ ابتدا replica set و سپس shard با key اندازه‌گیری‌شده | رشد سند و transaction چندسندی؛ skew shard key و هزینهٔ query بین shard |
| DynamoDB براساس access pattern | key lookup با scale مدیریت‌شدهٔ AWS می‌خواهیم و queryها معلوم‌اند | مدل partition key + sort key برای هر use case؛ GSI برای queryهای دیگر؛ ظرفیت on-demand یا provisioned؛ write شرطی/idempotent | hot partition، throttling/هزینهٔ capacity و وابستگی AWS؛ تعارض global table و محدودیت منطقه‌ای transaction |
| projectionهای polyglot | یک منبع حقیقت transaction را نگه می‌دارد ولی read تخصصی کم‌تأخیر می‌خواهیم | PostgreSQL + outbox/Kafka → view داغ Redis، سند MongoDB یا search index؛ projection versioned و replayپذیر | store و pipeline بیشتر و consistency lag؛ rebuild projection، مهاجرت schema، مشاهده‌پذیری و reconciliation ضروری است |

از ساده‌ترین دیتابیس authoritative که invariant را حفظ می‌کند شروع کن. cache/projection را فقط برای query اندازه‌گیری‌شده اضافه کن و مسیر rebuild idempotent از منبع حقیقت نگه دار.

## چه چیزهایی را اندازه بگیریم

latency هر عملیات در p50/p95/p99، throughput، index/write amplification، skew و throttling hot-key، بازهٔ از دست‌رفتن write تأییدشده، مدت failover/recovery، زمان restore backup و هزینهٔ کل سرویس/مهندسی.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## منابع

### مبنای کتاب

- Ian Gorton، *Foundations of Scalable Systems*، فصل 13: “Distributed Database Implementations” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch13.html).

### مستندات فنی بیرونی

- مستندات فنی اولیه: [مرجع رسمی](https://www.mongodb.com/docs/manual/sharding/).

### پیوندهای این مخزن

- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
