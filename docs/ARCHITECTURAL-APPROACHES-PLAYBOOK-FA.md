<div dir="rtl" align="right">

# راهنمای رویکردهای معماری برای سامانه‌های پرترافیک

این playbook رویکردها را بر اساس مسئلهٔ حل‌شده مقایسه می‌کند. آن‌ها قطعات جایگزین‌پذیرِ بی‌هزینه نیستند؛ هر کدام semantics، failure mode، عملیات و هزینهٔ خود را دارند. برای مثال و جریان end-to-end سبد و سفارش، [طرح‌های اجرایی](IMPLEMENTABLE-SOLUTION-BLUEPRINTS-FA.md) را ببین.

## ۱. قبل از انتخاب، مسئله را مشخص کن

برای هر operation بنویس: آیا پاسخ نهایی باید همین حالا حاضر باشد؟ چه consistency لازم است؟ ترتیب روی کدام entity لازم است؟ duplicate مجاز است؟ burst چقدر طول می‌کشد؟ چه حجمی از داده نگهداری می‌شود؟ چه کسی datastore و broker را اداره می‌کند؟ latency، availability، durability، هزینه و زمان بازیابی چه هدفی دارند؟

جریان پایهٔ مشترک برای مقایسه:

```text
Client command → validate/authenticate → persist accepted state
→ perform business effect → expose result/status → observe and recover
```

## ۲. رویکردها

### A. Modular monolith + relational database

**جریان:** client → ASP.NET Core/API → PostgreSQL transaction.

**پیاده‌سازی:** یک deployable، moduleهای دامنه‌ای با مرز کد و مالکیت جدول روشن؛ index و constraint؛ idempotency key در همان transaction؛ outbox فقط وقتی انتشار event لازم است.

**انتخاب کن وقتی:** تیم کوچک/متوسط است، invariantها چند entity را در یک DB درگیر می‌کنند و بار با tuning یا scale-up جواب می‌دهد.

**محدودیت:** scale و انتشار همهٔ moduleها یک‌جا است؛ writer، lock داغ یا حجم DB سقف می‌شود.

### B. Stateless synchronous services

**جریان:** Load balancer → stateless API replicas → transaction DB.

**پیاده‌سازی:** session را بیرون از process نگه دار؛ timeout و deadline، poolهای bounded، request idempotency، readiness/liveness و limit منابع تنظیم کن. read replica فقط برای readهای stale-tolerant.

**انتخاب کن وقتی:** caller نتیجهٔ قطعی فوری می‌خواهد و عملیات در یک مرز تراکنشی می‌گنجد.

**محدودیت:** همهٔ درخواست‌ها ظرفیت پایین‌دست را مستقیم مصرف می‌کنند؛ call chain و retry می‌توانند cascade بسازند.

### C. Work queue + worker pool

**جریان:** API → durable queue → competing consumers → DB/side effect.

**پیاده‌سازی:** API پس از durable publish پاسخ پذیرش بدهد؛ workerها ack/commit را پس از اثر پایدار انجام دهند؛ message id را در DB unique کنند؛ retry محدود، exponential backoff، DLQ، alert و replay tool داشته باشند.

**انتخاب کن وقتی:** taskها independent یا قابل‌تقسیم‌اند و async result پذیرفته شده است.

**محدودیت:** duplicate، lag و poison message؛ صف اگر ورودی همواره سریع‌تر از خروجی باشد نامحدود رشد می‌کند.

### D. Kafka event log / event-driven integration

**جریان:** producer → partitioned topic → چند consumer group → read models/services.

**پیاده‌سازی:** key را با aggregate ordering انتخاب کن؛ schema version/compatibility، retention, replication, min ISR, producer ack, consumer offset و replay را طراحی کن. Side effect خارجی idempotent باشد؛ monitoring شامل lag، oldest message age، rebalance و broker capacity باشد.

**انتخاب کن وقتی:** replay، چند consumer مستقل، جذب burst یا integration رویدادمحور ارزش دارد.

**محدودیت:** Kafka منطق را اجرا نمی‌کند و partition/consumer scaling را خودکار حل نمی‌کند؛ تعداد partition و ظرفیت downstream حد می‌گذارند.

### E. Transactional Outbox + CDC

**جریان:** یک DB transaction: business rows + outbox row → CDC از WAL → Kafka → idempotent consumers.

**پیاده‌سازی:** outbox row را در همان transaction بنویس؛ CDC lag و replication slot/WAL growth را monitor کن؛ event id و aggregate version را منتشر کن؛ cleanup و schema evolution تعریف کن.

**انتخاب کن وقتی:** business database منبع حقیقت است و event متناظر با commit نباید از دست برود.

**محدودیت:** connector/CDC عملیات تازه می‌آورد؛ duplicate و lag همچنان ممکن‌اند و consumer باید idempotent باشد.

### F. CQRS / read projections

**جریان:** command model → durable event/change → projector → query store/cache.

**پیاده‌سازی:** write model invariant را نگه دارد؛ query model برای الگوی read طراحی شود؛ version و lag در API/status نمایان باشد؛ projection replay و rebuildپذیر باشد.

**انتخاب کن وقتی:** read workload و شکل پاسخ از write model متفاوت است یا read scale جدا لازم است.

**محدودیت:** eventual consistency، duplicate pipeline، read model drift و rebuild time.

### G. Event Sourcing

**جریان:** command → aggregate validation/version check → append-only event store → projections.

**پیاده‌سازی:** expected version برای append، event schema version، snapshots برای replay، outbox/CDC برای انتشار، projection idempotency، correction/compensation و PII retention rules طراحی کن.

**انتخاب کن وقتی:** audit، بازسازی state و تغییر مداوم read model ارزش قابل‌اندازه‌گیری دارند.

**محدودیت:** versioning و replay و correction پیچیده است؛ برای CRUD ساده یا فقط داشتن Kafka انتخاب مناسبی نیست.

### H. Saga: choreography یا orchestration

**جریان نمونه:** order accepted → inventory reserve → payment authorize → confirm؛ در شکست، compensation معنادار اجرا شود.

**پیاده‌سازی:** state machine و deadline هر مرحله؛ idempotent command/event؛ timeout برای تشخیص وضعیت نامعلوم؛ retry محدود؛ compensation business؛ endpoint/status قابل‌پیگیری.

**Orchestration** یک coordinator روند را دنبال می‌کند؛ **choreography** سرویس‌ها را با event به هم متصل می‌کند. انتخاب را بر اساس observability، تعداد مرحله‌ها و نیاز مالکیت بگیر.

**محدودیت:** compensation rollback واقعی نیست؛ کاربر ممکن است intermediate state ببیند.

### I. Stream processing

**جریان:** Kafka source → keyed operators/window/join → checkpointed state → sink.

**پیاده‌سازی:** event time، watermark، late data، state backend، checkpoint interval، sink transaction/idempotency، parallelism و key skew را مشخص کن.

**انتخاب کن وقتی:** aggregation پیوسته، window، join جریان یا state پردازشی لازم است.

**محدودیت:** Flink/Kafka Streams جای تراکنش دقیق inventory را نمی‌گیرند؛ correctness end-to-end به sink و semantics آن وابسته است.

### J. Cache/CDN

**جریان:** read → cache lookup → miss coalescing → source → fill; write → source commit → invalidate/version.

**پیاده‌سازی:** دادهٔ cacheable، key, TTL, freshness, invalidation, privacy headers, stampede, eviction و fallback را ثبت کن.

**انتخاب کن وقتی:** read تکراری و نسبتاً کم‌تغییر و منبع پرهزینه است.

**محدودیت:** cache منبع truth نیست؛ cache outage می‌تواند source را overwhelm کند.

### K. Serverless functions

**جریان:** HTTP/event → managed invocation → function → external durable store.

**پیاده‌سازی:** idempotency، max concurrency، timeout/deadline، downstream connection cap، cold/warm benchmark، retry/DLQ و هزینهٔ هر کار موفق.

**انتخاب کن وقتی:** workload bursty/متناوب و invocation کوتاه است و نیاز provider fit می‌کند.

**محدودیت:** quota، cold start، provider lock-in، flood به دیتابیس و قیمت dynamic.

## ۳. چطور این رویکردها را ترکیب کنیم

نمونهٔ معقول برای سبد و سفارش:

```text
stateless API
  ├─ fast command acceptance → Kafka (partition key=basketId)
  ├─ basket worker → Redis projection (rebuildable, explicit TTL)
  └─ checkout/order transaction → PostgreSQL + idempotency + outbox
outbox CDC → Kafka → independent status/analytics consumers
Flink only for streaming analytics, not inventory authority
```

این ترکیب باید تدریجی ساخته شود: اول relational baseline، بعد بار واقعی و bottleneck؛ سپس queue/worker برای async workload؛ Outbox برای dual-write gap؛ projection وقتی read model یا scaling نیاز دارد؛ Flink فقط با نیاز window/join واقعی.

## ۴. زبان و runtime بر اساس بخش کار

- API I/O-bound: C#/.NET async، Go net/http، Java/Scala futures/virtual threads، Elixir processes یا Node event loop؛ انتخاب را با driverها و مهارت تیم benchmark کن.
- CPU-heavy: ابتدا profile و algorithm/vectorization؛ سپس Rust/C++ یا پردازش موازی JVM/.NET/Go را با هزینهٔ interoperability بسنج.
- Kafka worker: partition/key، batching، commit semantics و client library ممکن است از خود زبان بیشتر روی نتیجه اثر بگذارند.
- Stream job: JVM ecosystem (Scala/Java و Flink) انتخاب طبیعی برای مقایسه است؛ runtime warm-up/GC را گزارش کن.
- Concurrency زیاد و fault isolation: BEAM/Elixir گزینهٔ آزمایش است، نه پیش‌فرض برنده برای CPU-heavy work.

## ۵. ترتیب ساخت راه‌حل‌ها

1. قرارداد یکسان Basket/Order/Inventory، idempotency، ordering و invariantها را ثبت کن.
2. API + PostgreSQL را به‌عنوان baseline درست و اندازه‌پذیر بساز.
3. همان workload را در .NET/Kafka فعلی اجرا و failure semantics و lag را ثبت کن.
4. PostgreSQL Outbox/CDC را جدا بساز تا dual-write را مقایسه کند.
5. یک NoSQL variant بر اساس query مشخص (document یا key-value) اضافه کن؛ migration نام‌محور نساز.
6. CQRS/Event Sourcing را فقط برای نیاز audit/rebuild یا query متفاوت بساز.
7. یک stream analytics job و یک serverless worker را برای workload مشخص اضافه کن.
8. Hot path را بعد از profiler به Go/Rust/Scala/Elixir port کن و semantics را ثابت نگه دار.

برای همهٔ سناریوها raw result و hardware/config را ثبت کن؛ عددی را بدون run قابل‌بازتولید، ظرفیت اثبات‌شده ننام.

## ۶. منابع فنی برای مطالعه و پیاده‌سازی

- [Kubernetes: Deployments](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/) و [Horizontal Pod Autoscaling](https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/) برای استقرار و scale کردن سرویس‌ها.
- [Apache Kafka: Design](https://kafka.apache.org/documentation/#design) و [Consumer Configurations](https://kafka.apache.org/documentation/#consumerconfigs) برای پارتیشن، ماندگاری، گروه مصرف‌کننده و offset.
- [Debezium Outbox Event Router](https://debezium.io/documentation/reference/stable/transformations/outbox-event-router.html) برای الگوی outbox با CDC.
- [Microsoft: CQRS pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs) و [Event Sourcing pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/event-sourcing) برای کاربردها و هزینه‌های این دو الگو.
- [Microsoft: Scheduler-Agent-Supervisor pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/scheduler-agent-supervisor) برای هماهنگی فرایندهای طولانی؛ انتخاب saga همچنان به مرزهای تراکنش و compensation کسب‌وکار وابسته است.
- [Apache Flink: Stateful Stream Processing](https://nightlies.apache.org/flink/flink-docs-stable/docs/concepts/stateful-stream-processing/) برای state و checkpoint در پردازش جریانی.
- [AWS Lambda: scaling behavior](https://docs.aws.amazon.com/lambda/latest/dg/lambda-concurrency.html) برای محدودیت هم‌زمانی در اجرای serverless.
- [Redis: client-side caching and cache invalidation](https://redis.io/docs/latest/develop/clients/client-side-caching/) برای نمونه‌ای از ملاحظات cache و تازگی داده.

این منابع مستندات پیاده‌سازی‌اند، نه ادعای این‌که همهٔ این فناوری‌ها در مخزن فعلی اجرا شده‌اند.

</div>
