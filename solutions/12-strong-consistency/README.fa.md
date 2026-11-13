<div dir="rtl" align="right">

# فصل 12: سازگاری قوی

![نمودار یادگیری و آزمایش فصل](12-strong-consistency-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

مدل‌های consistency؛ تراکنش توزیع‌شده؛ 2PC؛ consensus؛ انتخاب رهبر و مصالحه‌های عملی.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 223–245.** این جمع‌بندی با بیان خودمان نوشته شده است؛ برای شرح کامل به کتاب مراجعه کن.

فصل دو مفهومی را جدا می‌کند که هر دو گاهی «strong consistency» نامیده می‌شوند. سازگاری تراکنشی یعنی transaction چندموردی invariant کسب‌وکار را اتمی نگه می‌دارد؛ سازگاری replica یعنی همهٔ مشتری‌ها مستقل از replica مقدار commitشدهٔ یکسان ببینند. ACID شامل atomicity، سازگاری تعریف‌شده با منطق کسب‌وکار، isolation و durability است. serializability یعنی transactionهای همزمان معادل یک ترتیب سری باشند؛ linearizability علاوه بر آن ترتیب زمان واقعی را برای عملیات روی یک شیء رعایت می‌کند. ترکیبشان strict serializability/external consistency می‌دهد، اما هماهنگی می‌خواهد.

two-phase commit (2PC) تراکنش توزیع‌شده را هماهنگ می‌کند: شرکت‌کننده‌ها ابتدا prepare می‌شوند و تضمین می‌کنند می‌توانند commit کنند؛ سپس coordinator نتیجهٔ commit یا abort را اعلام می‌کند. خرابی شرکت‌کننده از روی log قابل بازیابی است، ولی خرابی coordinator بعد از prepare ممکن است شرکت‌کننده‌ها را در حالی که lock نگه داشته‌اند منتظر نتیجه بگذارد. این کار availability را کم و ممکن است timeoutهای بالادست را زنجیره‌ای کند. تکثیر وضعیت coordinator و بازیابی آن به consensus نیاز دارد، نه صرفاً retry.

consensus بین replicaها ترتیب یکسان update می‌سازد. Raft الگوریتم log تکثیرشده با رهبر است: leader نوشتن‌های client را مرتب می‌کند، followerها entry را پایدار می‌کنند و quorum اکثریت آن را commit می‌کند. heartbeat نبود رهبر را نشان می‌دهد؛ candidate با term، تازگی log و رأی اکثریت leader می‌شود. quorum اجازه می‌دهد پس از برخی خرابی nodeها ادامه دهیم، اما بدون اکثریت پیشروی متوقف است. replication و انتخاب رهبر round-trip شبکه می‌خواهند و هنگام بازیابی ممکن است write مکث کند؛ Raft/Paxos با فرض‌های معین توافق می‌سازند، نه در برابر partition دائمی دلخواه.

مثال‌ها روش‌های متفاوتی برای کم‌کردن یا مدیریت هماهنگی نشان می‌دهند. VoltDB پارتیشن را به CPU core وصل و stored procedureها را در هر پارتیشن سری اجرا می‌کند؛ transaction تک‌پارتیشن از 2PC بین‌پارتیشنی دوری می‌کند، ولی کار چندپارتیشنی پرهزینه‌تر است. چون داده در حافظه است، command log و snapshot برای durability لازم‌اند. Spanner جدول را به splitهای بازه‌ای تکثیرشده تقسیم می‌کند، برای هر گروه replica از Paxos و بین splitها از 2PC استفاده می‌کند و با timestamp commit، سرویس زمان با خطای محدود و commit-wait، external consistency می‌دهد. تضمین قوی، stale-read و تعارض‌های برنامه را کم می‌کند، اما دیتابیس هزینهٔ هماهنگی، lock، latency و پیچیدگی عملیاتی را می‌پردازد. transactionهای قوی را کوچک نگه دار و در صورت امکان partition key را با مرز transactionهای متداول هم‌راستا کن.

### نکته‌های کلیدی

- مدل consistency و invariant را دقیق تعریف کن.
- بازیابی two-phase commit و فرض‌های consensus را بشناس.
- هماهنگی را جایی به‌کار ببر که صحت لازم دارد و هزینهٔ آن را بسنج.

### پوشش موضوع‌های اصلی فصل

- ACID، serializability، linearizability و strict/external consistency.
- تراکنش توزیع‌شده با 2PC prepare/resolve، بازیابی participant و پیامدهای توقف coordinator/lock.
- safety/liveness در consensus، log ماشین حالت تکثیرشده، term/quorum/election در Raft و جایگاه Paxos.
- transaction تک‌پارتیشنی/چندپارتیشنی VoltDB و split replication، Paxos، 2PC و commit-wait در Spanner.

## درس آموزشی

### هدف‌های یادگیری و نقشهٔ مفهومی

- ACID، serializability، linearizability و replica consistency را تفکیک کنیم.
- 2PC را در مسیر موفق و خرابی coordinator دنبال کنیم.
- Raft و quorum را در حد معماری و failure mode توضیح دهیم.

`invariant → transaction isolation → replication protocol → commit/visibility → failure recovery`

### چه تضمینی می‌خواهیم؟

ACID transaction مجموعه تغییرها را اتمی، مطابق constraint کسب‌وکار، ایزوله از transactionهای هم‌زمان و پس از commit durable می‌کند. **Serializability** می‌گوید نتیجهٔ اجرای هم‌زمان مثل اجرای ترتیبی برخی transactionها باشد؛ **linearizability** برای عملیات یک شیء ترتیب واقعی زمان را نیز رعایت می‌کند. این دو مترادف نیستند. دیتابیس ممکن است تراکنش ACID در primary داشته باشد اما read از replica عقب‌مانده بخواند؛ transaction consistency با replica consistency یکی نیست.

### 2PC و خطر انتظار

در Two-phase commit، coordinator از participantها prepare می‌خواهد؛ participant منابع/log را آماده می‌کند و رأی می‌دهد. سپس coordinator تصمیم commit/abort را اعلام و ثبت می‌کند. اگر coordinator بعد از prepare از دسترس خارج شود، participant نمی‌تواند همیشه با اطمینان تصمیم را حدس بزند و ممکن است قفل نگه دارد. 2PC atomicity را هماهنگ می‌کند اما availability در failure و tail latency را تحت تأثیر می‌گذارد. Retry پاسخ نامعلوم، transaction را خودکار امن نمی‌کند.

### Consensus و Raft

Leader ورودی‌ها را در log مرتب می‌کند؛ followerها کپی می‌گیرند و پس از ثبت روی اکثریت، entry commit است. Heartbeat و election leader تازه را انتخاب می‌کنند، و term/log freshness از رأی به log عقب‌مانده جلوگیری می‌کند. با partition شبکه، side دارای quorum می‌تواند جلو برود و طرف بدون اکثریت باید نوشتن را متوقف کند تا split-brain رخ ندهد. quorum برای دسترسی availability هزینه دارد و write ممکن است round-trip اضافی بخواهد. Consensus ترتیب مشترک/replica state می‌سازد؛ خودش transaction منطقی application را طراحی نمی‌کند.

### مثال invariant چندردیفی

انتقال ۱۰ واحد از حساب A به B باید هر دو تغییر را با هم commit یا abort کند؛ جدا کردن دو write و ارسال یک event بدون طراحی saga می‌تواند پول را یک‌طرفه کم کند. اگر state در یک DB است، transaction محلی ساده‌تر است. اگر دو سرویس/DB مالک حساب‌ها هستند، باید contract توزیع‌شده، idempotency و compensation یا consensus transaction را آگاهانه انتخاب کرد.

### تصمیم و تمرین

| انتخاب | تضمین | هزینه/کاربرد |
| --- | --- | --- |
| Transaction یک DB | invariant چندردیفی محلی | simplest correctness baseline |
| Leader + synchronous quorum | read/write تازه‌تر تا quorum | latency و توقف بدون quorum |
| 2PC | atomic commit بین participants | coordinator state/locking/failure handling |
| Saga | workflow eventual با compensation | intermediate states و business reversal |

تمرین: 2PC trace را در participant failure، coordinator failure قبل prepare و بعد prepare رسم کن. سپس Raft cluster را با 3 node تصور کن: کدام nodeها می‌توانند commit را تأیید کنند و رفتار client سمت minority چیست؟

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `StrongConsistencyTransactionLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter12
- **پوشهٔ مستقل راه‌حل:** solutions/12-strong-consistency/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

عملیات رزرو موجودی و ثبت سفارش را بساز. transaction اتمی PostgreSQL، transaction دیتابیس SQL توزیع‌شده بین دو partition، و جایگزین saga ناهمزمان/outbox را مقایسه کن. invariant ثابت باشد: سفارش پذیرفته‌شده بیشتر از موجودی رزرو نکند و سفارش پرداخت‌شده اعلام نشود مگر مرحلهٔ پرداخت موفق شده باشد.

۱. پیش از انتخاب دیتابیس، دامنهٔ transaction، isolation level، partition key و invariant دقیق را تعریف کن.
۲. رزرو همزمان روی یک key و بین keyهای چند partition اجرا کن؛ عدم oversell و latency تک‌پارتیشنی/چندپارتیشنی را بسنج.
۳. خرابی participant/node را پیش از prepare، پس از prepare و حین commit تزریق کن. ماندن lock، retry، resolve و نتیجهٔ قابل‌مشاهده برای client را ببین.
۴. latency و throughput موفق را با زمان recovery و availability هنگام از دست‌رفتن quorum مقایسه کن. درخواست‌ها را با idempotency key retry کن.
۵. در saga، هر مرحله و compensation را پایدار کن، همهٔ مرحله‌ها را idempotent بساز و وضعیت pending/failed را نشان بده؛ وانمود نکن کل workflow یک transaction اتمی است.

### انتخاب‌های راه‌حل قابل پیاده‌سازی

| راه‌حل | چه زمانی انتخابش کنیم | شکل اجرایی | هزینه‌ها و محدودیت‌ها |
|---|---|---|---|
| transaction رابطه‌ای واحد | invariantها در یک دیتابیس جا می‌شوند و حجم درخواست در یک primary می‌گنجد | .NET API → transaction در PostgreSQL که موجودی را رزرو و سفارش/payment intent را اتمی ثبت می‌کند | صحت ساده‌تر؛ سقف write و availability primary باقی است |
| transaction SQL توزیع‌شده | یک invariant اتمی بین partition/region امتداد دارد و نتیجهٔ stale/نیمه‌کاره پذیرفتنی نیست | SQL مانند CockroachDB/YugabyteDB/Cloud Spanner؛ تا حد امکان partition بر order/tenant و محدودکردن کار بین partitionها | consensus و 2PC latency می‌افزایند؛ نبود quorum نوشتن را متوقف می‌کند؛ هزینهٔ سرویس/ابر |
| saga ناهمزمان با outbox | workflow از سرویس/providerهای مستقل عبور می‌کند و تکمیل نهایی با تأخیر قابل‌قبول است | Order DB + outbox → orchestrator → رزرو موجودی → مجوز پرداخت → تأیید؛ در شکست release/void جبرانی | transaction واحد ACID نیست؛ وضعیت pending، recovery timeout، idempotency و reconciliation لازم است |

transaction توزیع‌شده را فقط وقتی به‌کار ببر که invariant واقعاً بین participantهاست و بودجهٔ latency/availability اجازه می‌دهد. در غیر این صورت transaction محلی همراه workflow ناهمزمان معمولاً ساده‌تر اداره می‌شود.

## چه چیزهایی را اندازه بگیریم

latency commit بر حسب تعداد partition، نرخ abort/retry، مدت lock، زمان توقف هنگام خرابی coordinator/leader، availability quorum، زمان recovery و نقض invariant.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 12: “Strong Consistency” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch12.html).
- مستندات فنی اولیه: [مرجع رسمی](https://raft.github.io/raft.pdf).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
