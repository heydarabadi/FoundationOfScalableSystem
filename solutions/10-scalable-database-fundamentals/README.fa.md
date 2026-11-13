<div dir="rtl" align="right">

# فصل 10: مبانی پایگاه‌دادهٔ مقیاس‌پذیر

![نمودار یادگیری و آزمایش فصل](10-scalable-database-fundamentals-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

scale-up/out رابطه‌ای؛ read replica؛ partitioning؛ مدل‌های NoSQL؛ توزیع و trade-offهای CAP.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 185–203.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

پایگاه‌دادهٔ توزیع‌شده با افزودن گره ظرفیت پردازش یا ذخیره‌سازی را بالا می‌برد، اما معنای دسترسی داده را تغییر می‌دهد. relational می‌تواند scale-up شود، read replica بگیرد یا داده را partition کند؛ هرکدام روی write، مسیریابی و کار cross-partition اثر دارند. NoSQL شامل مدل‌های گوناگون key-value، document و موارد دیگر با query و توزیع متفاوت است. data model و partition key پخش بار و hot spot را تعیین می‌کنند. CAP برای تحلیل رفتار هنگام partition شبکه است، نه امتیاز عمومی محصول. انتخاب را با access pattern، consistency و failure انجام بده.

### نکته‌های کلیدی

- پیش از replica یا partition، baseline relational را بشناس.
- NoSQL را بر اساس مدل و الگوی دسترسی انتخاب کن.
- partition key توزیع و hot key را تعیین می‌کند؛ CAP دربارهٔ تضمین هنگام partition است.

### پوشش موضوع‌های اصلی فصل

- نیاز پایگاه‌دادهٔ توزیع‌شده و scale-up، read replica و partition در relational.
- نمونهٔ clustering رابطه‌ای، مدل‌های NoSQL، زبان query و توزیع داده.
- CAP برای تحلیل partition شبکه و تضمین‌هاست، نه رتبه‌بندی محصول.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `ScalableDatabaseFundamentalsLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter10
- **پوشهٔ مستقل راه‌حل:** solutions/10-scalable-database-fundamentals/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Keep one transactional workload constant; compare a tuned relational baseline with read scaling or partitioning only when a bottleneck is measured.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

Query latency, write throughput, replication lag, hot-key skew, storage growth, and operational cost.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 10: “Scalable Database Fundamentals” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch10.html).
- مستندات فنی اولیه: [مرجع رسمی](https://www.postgresql.org/docs/current/ddl-partitioning.html).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
