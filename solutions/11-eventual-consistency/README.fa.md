<div dir="rtl" align="right">

# فصل 11: سازگاری نهایی

![نمودار یادگیری و آزمایش فصل](11-eventual-consistency-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

پنجرهٔ ناسازگاری؛ read-your-writes؛ consistency قابل تنظیم؛ quorum؛ ترمیم replica و حل تعارض.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 205–221.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

سازگاری نهایی اختلاف موقت replicaها را در زمان انتشار update می‌پذیرد. پنجرهٔ ناسازگاری بر رفتار کاربر اثر می‌گذارد؛ read-your-writes می‌تواند تضمین محلی بدهد بدون آن‌که توافق فوری جهانی باشد. consistency قابل تنظیم و quorum read/write تعداد replicaهای مشارکت‌کننده را عوض می‌کند و روی latency و تازگی اثر دارد. replica repair updateهای ازدست‌رفته را هماهنگ می‌کند. write هم‌زمان تعارض می‌سازد: last-writer-wins ساده است ولی ممکن است نیت را حذف کند؛ version vector هم‌زمانی را آشکار می‌کند. برنامه باید staleness قابل قبول و حل تعارض درست در دامنه را تعریف کند.

### نکته‌های کلیدی

- سازگاری نهایی قرارداد قابل مشاهدهٔ برنامه است.
- quorum میان latency و تازگی هنگام خرابی trade-off دارد.
- حل تعارض باید نیت دامنه را حفظ کند، نه فقط timestamp را مقایسه کند.

### پوشش موضوع‌های اصلی فصل

- سازگاری نهایی و پنجرهٔ ناسازگاری، از جمله read-your-writes.
- consistency قابل تنظیم و quorum read/write؛ repair فعال و غیرفعال replica.
- حل تعارض با last-writer-wins و version vector و مصالحه‌های هرکدام.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `EventualConsistencyRecoveryLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter11
- **پوشهٔ مستقل راه‌حل:** solutions/11-eventual-consistency/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Simulate replicated reads and delayed updates; write down which stale reads the product can tolerate and how clients recover.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

Staleness duration, convergence time, conflict count, read-your-writes success, and availability during partition.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 11: “Eventual Consistency” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch11.html).
- مستندات فنی اولیه: [مرجع رسمی](https://cassandra.apache.org/doc/latest/cassandra/architecture/dynamo.html).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
