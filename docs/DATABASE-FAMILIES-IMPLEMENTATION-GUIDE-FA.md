<div dir="rtl" align="right">

# راهنمای آموزشی انتخاب و پیاده‌سازی خانواده‌های پایگاه‌داده

این سند راهنمای مکمل فصل‌های ۱۰ تا ۱۳ کتاب است. هدفش این نیست که یک محصول را «بهترین» اعلام کند؛ می‌خواهیم مسئلهٔ داده، دسترسی، consistency، دوام، مقیاس و هزینه را به انتخاب قابل‌آزمایش تبدیل کنیم. جزئیات نسخه و سرویس‌های مدیریت‌شده تغییر می‌کنند؛ پیش از اجرا، مستند همان نسخه را بررسی کن.

## ۱. از نیاز به خانوادهٔ داده برسیم

پیش از انتخاب موتور، برای هر عملیات بنویس:

1. **شکل داده:** رابطه‌ای و دارای constraint است، سندی است، گراف است، یا event/series زمانی؟
2. **الگوی دسترسی:** lookup با کلید، join و گزارش، جست‌وجوی متن، traversal رابطه‌ها یا aggregation پنجره‌ای؟
3. **تضمین:** چه چیزی باید اتمی باشد؟ stale read قابل قبول است؟ RPO و RTO چقدرند؟
4. **رشد:** نرخ read/write، اندازهٔ working set، retention، hot key و burst چیست؟
5. **عملیات:** backup/restore، failover، migration، observability و هزینهٔ نگهداری را چه کسی اداره می‌کند؟

این جدول نقطهٔ شروع است، نه حکم قطعی:

| خانواده | مدل اصلی | شروع مناسب برای | گلوگاه یا خطر متداول |
|---|---|---|---|
| رابطه‌ای / OLTP | جدول، constraint، join و transaction | سفارش، پرداخت، موجودی | contention روی ردیف داغ، query/index ضعیف، اتصال بیش از ظرفیت |
| Key-value / cache | کلید به مقدار یا ساختار | session، cache، counter، lookup قطعی | کلید نامتوازن، eviction، durability اشتباه‌فهمیده‌شده |
| Document | سندهای تو‌در‌تو | aggregate با دسترسی سندی و schema متغیر | denormalization، سند پررشد، transaction چندسندی |
| Wide-column | partition key و clustering columns | write حجیم و lookupهای قابل‌پیش‌بینی در مقیاس افقی | طراحی query-first، hot partition، consistency و repair |
| Distributed SQL | SQL با توزیع/replication داخلی | نیاز هم‌زمان به SQL و scale چندگرهی | latency تراکنش، conflict/retry، عملیات پیچیده‌تر |
| Graph | node و edge | چند-پرشیِ رابطه‌ها، مسیر و ارتباط | queryهای بدون نقطهٔ شروع/index، مدل‌سازی و حافظه |
| Search / inverted index | token و document index | متن، relevance، faceting و filtering | index منبع حقیقت نیست؛ refresh و بازسازی هزینه دارد |
| Time-series / analytical OLAP | دادهٔ زمان‌دار ستونی/تجمیعی | metrics، رویدادهای حجیم و گزارش | ingestion و queryهای تعاملی با OLTP قاطی می‌شوند |
| Object store / lake | فایل/شیء ارزان و schema-on-read | تاریخچه، archive و تحلیل دسته‌ای | latency، catalog، schema evolution و governance |

## ۲. روش مشترک پیاده‌سازی و سنجش

برای هر variant یکسان نگه دار: API و قرارداد کسب‌وکار، seed data، حجم داده، بار، idempotency، مدت آزمون، سخت‌افزار و نسخهٔ client. قبل از benchmark، correctness را بررسی کن. برای هر موتور این‌ها را ثبت کن:

- schema و access pattern واقعی، indexها و query plan؛
- durability و replication config، backup و restore drill؛
- p50/p95/p99، throughput موفق، error rate، CPU، RAM، disk/network و cost؛
- رفتار هنگام restart، failover، hot key، افزایش بار و خراب‌شدن client؛
- projection lag یا replication lag، میزان دادهٔ از دست‌رفته و زمان بازیابی.

`TPS` را تعریف کن: فرمان پذیرفته‌شده، commit دیتابیس یا عملیات کسب‌وکار نهایی؟ این سه عدد را یکی نگیریم.

## ۳. رابطه‌ای: PostgreSQL و MySQL

### چه زمانی

وقتی foreign key، unique constraint، join، تراکنش چندردیفی و queryهای متنوع بخشی از صحت دامنه‌اند. برای checkout و رزرو موجودی، relational معمولاً baseline با semantics روشن است. ابتدا schema و queryها را درست کن؛ قبل از partition/shard، slow query، lock wait، WAL/redo، pool و storage را اندازه بگیر.

### الگوی موجودی

```sql
BEGIN;
UPDATE inventory
   SET available = available - :quantity
 WHERE sku = :sku AND available >= :quantity;
-- برنامه باید بررسی کند دقیقاً یک ردیف تغییر کرده باشد.
INSERT INTO orders(order_id, request_id, status)
VALUES (:order_id, :request_id, 'accepted');
COMMIT;
```

برای چند SKU، همهٔ ردیف‌ها و order را در یک transaction به‌روز کن؛ ترتیب lock را ثابت نگه دار و deadlock را retry محدود کن. اگر `request_id` یکتا است، ثبت آن را همان‌جا و اتمی انجام بده تا درخواست تکراری نتیجهٔ قبلی را بگیرد. isolation level، constraint و lock باید با invariant واقعی طراحی شوند.

### رشد

از index بر اساس query plan، query batching، connection pool محدود، read replica برای readهای stale-tolerant و partition زمانی برای retention شروع کن. partitioning به معنی replica یا تقسیم خودکار write نیست. shard کردن اپلیکیشن، cross-shard transaction و unique constraint سراسری را دشوار می‌کند؛ فقط بعد از اثبات bottleneck و انتخاب shard key اقدام کن.

PostgreSQL از MVCC برای snapshotهای خواندن و نوشتن هم‌زمان استفاده می‌کند و ابزار lock هم دارد. MySQL با InnoDB هم transaction و locking/versioning دارد؛ isolation و lock behavior آن را از PostgreSQL عیناً فرض نکن. تفاوت‌ها را با query و workload هدف آزمایش کن.

## ۴. Key-value، cache و Redis

### چه زمانی

Redis برای دسترسی کم‌تأخیر به کلید معلوم، ساختارهایی مثل hash/list/set/sorted set، cache و projection مناسب است. برای cache، TTL، eviction، invalidation، stampede و fallback به source of truth را تعریف کن. برای دادهٔ غیرقابل‌بازسازی، persistence، backup، replication و RPO را جداگانه مشخص کن؛ «در RAM است» یا «AOF روشن است» به‌تنهایی طرح دوام کامل نیست.

```text
GET basket:{basketId}
MISS -> read authoritative store -> SET basket:{basketId} value EX 300
WRITE -> update store -> invalidate/update projection with version
```

موجودی پولی را فقط در Redis نگه ندار مگر قرارداد durability و recovery کاملاً طراحی و آزموده شده باشد. Redis cluster/shard ممکن است چندکلیدی را محدود کند؛ key design را با عملیات لازم بسنج.

### DynamoDB و key-value/document مدیریت‌شده

برای دسترسی‌هایی که با partition key و sort key قابل‌مدل‌سازی‌اند، DynamoDB می‌تواند عملیات مدیریت زیرساخت را کم کند. دسترسی اصلی را حول کلیدها و secondary indexهای لازم طراحی کن؛ partition key کم‌تنوع یا hot key throughput را نامتوازن می‌کند. شرط‌گذاری و transaction را در جایی مصرف کن که invariant چند آیتمی واقعاً نیاز دارد و محدودیت و هزینه‌اش را در طراحی حساب کن. query آزاد شبیه SQL فرض نکن.

## ۵. Document: MongoDB

سندهای BSON و embedding می‌توانند aggregateهایی را که با هم خوانده/نوشته می‌شوند در یک واحد اتمی نگه دارند. مدلی را حول queryهای پرتکرار طراحی کن؛ denormalize کردن read را ساده می‌کند ولی duplicate data و update fan-out می‌آورد. سندی با آرایهٔ بدون حد نساز. operation تک‌سندی و transaction چندسندی را یکی ندان؛ transaction توزیع‌شده هزینه دارد و جای schema مناسب را نمی‌گیرد.

برای sharding، shard key باید cardinality و توزیع بار مناسب و query routing قابل استفاده داشته باشد. انتخاب نامناسب باعث hotspot یا scatter-gather می‌شود. برای هر collection، index و unique constraint لازم را تعریف کن؛ replication و write concern را با RPO/latency هدف تطبیق بده.

```javascript
db.inventory.updateOne(
  { sku: "SKU-42", available: { $gte: 2 } },
  { $inc: { available: -2 } }
)
```

در برنامه، `matchedCount`/نتیجه را بررسی کن و در صورت نیاز چند سند باید با هم تغییر کنند، transaction و retry semantics مناسب را طراحی کن.

## ۶. Wide-column: Cassandra

Cassandra برای partitionشده و replicated کردن داده با availability و scale-out طراحی شده است. در آن ابتدا queryها را فهرست کن و بعد tableها را برای query بساز؛ یک جدول canonical و join آزاد مانند relational تصور نکن. Partition key تعیین می‌کند داده و بار به کجا برود؛ clustering columns ترتیب داخل partition را می‌سازند. partition بیش از حد بزرگ و کلید داغ خطرند.

Replication factor تعداد نسخه‌هاست؛ consistency level تعداد پاسخ‌های موردنیاز هر عملیات را کنترل می‌کند. eventual consistency، clock-based conflict resolution، repair و tombstoneها باید فهمیده شوند. LWT با Paxos برای compare-and-set قوی‌تر است ولی هزینهٔ latency بیشتری دارد. Cassandra را برای تراکنش‌های چندموجودیتی با قید سراسری انتخاب نکن مگر دامنه را به partitionهای مستقل محدود کرده باشی.

## ۷. Distributed SQL

CockroachDB/YugabyteDB و گزینه‌های مشابه برای SQL و تراکنش توزیع‌شده طراحی شده‌اند. مدل ذهنی SQL داشتن به معنی latency تک‌گره‌ای نیست: transaction ممکن است شبکه و quorum را طی کند و serialization conflict نیازمند retry کل transaction باشد. location, locality, transaction size و hotspot را در آزمون واقعی بررسی کن. قبل از مهاجرت، schema/query compatibility، connection pool، backup، upgrade و runbook شکست را بسنج.

## ۸. Graph، Search و تحلیل زمانی

- **Graph DB:** وقتی پاسخ از پیمایش چندمرحله‌ای رابطه‌ها می‌آید، node/edge و traversal مدل طبیعی‌تری از joinهای زنجیره‌ای‌اند. constraint و index روی نقطهٔ شروع لازم است؛ query بی‌محدودیت می‌تواند زیرگراف بزرگی را بخواند.
- **Search engine:** متن را token/index می‌کند و برای relevance، facet و جست‌وجو مناسب است؛ معمولاً index مشتق از دیتابیس است. تغییر اصلی را در منبع حقیقت ذخیره و index را با CDC/outbox یا pipeline همگام کن. بازسازی و eventual freshness را برنامه‌ریزی کن.
- **Time-series/OLAP:** telemetry و eventهای زمان‌دار را با partitioning زمانی، فشرده‌سازی و aggregation نگه می‌دارد. برای هر query بازهٔ زمان و granularity تعریف کن؛ این موتور معمولاً جای تراکنش‌های inventory را نمی‌گیرد.
- **Data lake/object store:** دادهٔ تاریخی چندقالبی را ارزان نگه می‌دارد؛ catalog، schema evolution، access control، lifecycle، retention و deletion را طراحی کن. معمولاً برای interactive OLTP و latency میلی‌ثانیه‌ای نیست.

## ۹. چهار طرح قابل ساخت برای یک مسئله

1. **Transactional baseline:** ASP.NET Core یا Go + PostgreSQL؛ checkout و inventory در transaction، idempotency با unique constraint، Redis فقط cache بازسازی‌پذیر.
2. **Document aggregate:** MongoDB؛ یک سند/aggregate برای مرزهایی که باید اتمی تغییر کنند، projection مجزا برای گزارش؛ شارد بر اساس tenant یا aggregate بعد از آزمون توزیع.
3. **Key-first serverless:** DynamoDB؛ partition/sort key بر اساس خواندن/نوشتن اصلی، conditional update برای رزرو، idempotency item و queue-triggered worker با concurrency محدود.
4. **Write-heavy distributed:** Cassandra برای رویداد/وضعیت‌هایی که حول partition key مستقل‌اند؛ Kafka برای ingest/replay، Cassandra به‌عنوان read state و PostgreSQL جدا برای تراکنش‌هایی که قید چندردیفی واقعی می‌خواهند.

هیچ طرحی را با «نام DB» کامل حساب نکن: schema، migration، backup/restore، connection/driver، observability، workload و failure test هم باید اجراپذیر باشند.

## تمرین جامع انتخاب و پیاده‌سازی

یک قرارداد کوچک انتخاب کن: `CreateOrder`, `GetOrder`, `ReserveStock`, `GetOrderStatus`. ابتدا PostgreSQL را با transaction و unique idempotency key پیاده کن؛ سپس یک variant document یا key-value را فقط برای access patternی که واقعاً با آن بهتر می‌شود بساز. در هر variant این کارها را انجام بده:

1. schema/key design، migration، indexها و constraintها را ثبت کن.
2. seed یکسان و workload یکسان با read/write mix ثابت بساز.
3. تکرار request، رقابت دو درخواست برای آخرین موجودی، process restart و replica/node failure را امتحان کن.
4. backup را به محیط خالی restore کن و نتیجه را با invariantهای کسب‌وکار تطبیق بده.
5. p50/p95/p99، عملیات موفق، خطا، CPU/RAM/IO، lag، زمان recovery و هزینه را گزارش کن.

این تمرین را می‌توان برای PostgreSQL/MySQL، Redis، MongoDB و DynamoDB اجرا کرد؛ Cassandra، graph و search را با workload مناسب همان خانواده بساز و ادعای برابری access pattern نکن.

## ۱۰. منابع رسمی

- [PostgreSQL MVCC](https://www.postgresql.org/docs/current/mvcc-intro.html) و [partitioning](https://www.postgresql.org/docs/current/ddl-partitioning.html)
- [MySQL InnoDB transaction model](https://dev.mysql.com/doc/refman/8.4/en/innodb-transaction-model.html)
- [Redis data types](https://redis.io/docs/latest/develop/data-types/) و [persistence](https://redis.io/docs/latest/operate/oss_and_stack/management/persistence/)
- [MongoDB transactions](https://www.mongodb.com/docs/manual/core/transactions/) و [sharding](https://www.mongodb.com/docs/manual/sharding/)
- [DynamoDB partitions](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/HowItWorks.Partitions.html) و [transactions](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/transactions.html)
- [Cassandra guarantees](https://cassandra.apache.org/doc/latest/cassandra/architecture/guarantees.html)
- [CockroachDB developer basics](https://www.cockroachlabs.com/docs/stable/developer-basics.html)
- [Neo4j graph transactions](https://neo4j.com/docs/operations-manual/current/database-internals/)
- [OpenSearch shards and replicas](https://docs.opensearch.org/latest/getting-started/intro/)

</div>
