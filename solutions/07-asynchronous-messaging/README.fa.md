<div dir="rtl" align="right">

# فصل 07: پیام‌رسانی ناهمگام

![نمودار یادگیری و آزمایش فصل](07-asynchronous-messaging-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

ماندگاری؛ pub/sub؛ queue؛ replication؛ competing consumers؛ ایمنی تحویل؛ پیام ناسالم و idempotency.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 127–144.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

پیام‌رسانی ناهمگام producer را از زمان و محل پردازش جدا می‌کند. persistence، publish-subscribe، replication، exchange و queue مسیر و نگه‌داری را مشخص می‌کنند. competing consumer کار را پخش می‌کند، اما acknowledgement، ترتیب، retry و idempotency صحت و کارایی را تعیین می‌کنند. replication دوام و availability را با هزینهٔ منبع و latency بهتر می‌کند. ادعای exactly-once فقط در مرزی مشخص معتبر است؛ تحویل تکراری ممکن است اثر end-to-end داشته باشد. پیام poison به جداسازی و سیاست بازیابی روشن نیاز دارد.

### نکته‌های کلیدی

- تضمین تحویل را از تضمین اثر کسب‌وکار جدا کن.
- persistence و replication را بر اساس شکست مورد انتظار انتخاب کن.
- retry را idempotent و بازیابی پیام poison را قابل مشاهده کن.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `ReliableAsyncMessagingLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter07
- **پوشهٔ مستقل راه‌حل:** solutions/07-asynchronous-messaging/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Compare synchronous processing with a durable queue and competing consumers; include retry, dead-letter, and graceful stop.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

Publish/consume rate, lag, delivery failures, redelivery, drain time, and durable completion.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 07: “Asynchronous Messaging” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch07.html).
- مستندات فنی اولیه: [مرجع رسمی](https://kafka.apache.org/documentation/).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
