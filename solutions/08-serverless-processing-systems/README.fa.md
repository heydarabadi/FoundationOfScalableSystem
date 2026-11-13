<div dir="rtl" align="right">

# فصل 08: سامانه‌های پردازش Serverless

![نمودار یادگیری و آزمایش فصل](08-serverless-processing-systems-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

چرخهٔ invocation؛ autoscaling؛ cold start؛ کنترل هم‌روندی؛ پارامترهای throughput و هزینه.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 147–161.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

serverless اجرای مدیریت‌شده‌ای می‌دهد که با تقاضا scale می‌شود و برای بار ناپیوسته یا جهشی مناسب است. فصل Google App Engine و AWS Lambda، autoscaling، چرخهٔ عمر function و ملاحظات اجرا را بررسی می‌کند. تأخیر startup، محدودیت اجرا، concurrency، state بیرونی و محدودیت پلتفرم بر throughput و tail latency اثر دارند. serverless ظرفیت بیکار و بخشی از عملیات را کم می‌کند، اما طراحی وابستگی، داده، retry و هزینه را حذف نمی‌کند. پارامترها را با آزمایش تکرارپذیر throughput و هزینه تنظیم کن.

### نکته‌های کلیدی

- serverless را با شکل بار و زمان بیکاری تطبیق بده.
- startup، محدودیت اجرا، state بیرونی و concurrency وابستگی را حساب کن.
- تنظیم را با throughput، latency و هزینهٔ اندازه‌گیری‌شده انجام بده.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `ServerlessThroughputCostLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter08
- **پوشهٔ مستقل راه‌حل:** solutions/08-serverless-processing-systems/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Compare a queue-triggered function with an always-on worker for the same idempotent task and arrival pattern.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

Cold-start latency, throughput, concurrency, retries, cost per completed task, and backlog recovery.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 08: “Serverless Processing Systems” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch08.html).
- مستندات فنی اولیه: [مرجع رسمی](https://docs.aws.amazon.com/lambda/latest/dg/welcome.html).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
