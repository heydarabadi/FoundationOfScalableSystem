<div dir="rtl" align="right">

# فصل 09: Microservices

![نمودار یادگیری و آزمایش فصل](09-microservices-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

تقسیم سرویس؛ مرز monolith؛ استقرار؛ تاب‌آوری؛ cascading failure و bulkhead.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 163–180.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

microservice برنامه را به سرویس‌های مستقل از نظر استقرار تقسیم می‌کند، اما هزینهٔ شبکه و عملیات دارد. مرز باید از قابلیت منسجم کسب‌وکار و مالکیت تیم پیروی کند؛ وگرنه پیچیدگی محلی به choreography راه‌دور شکننده تبدیل می‌شود. انتشار مستقل به تیم کمک می‌کند، ولی هر تماس راه‌دور latency و شکست جزئی می‌آورد. resilience باید cascading failure را مهار کند؛ bulkhead منابع را جدا می‌کند تا یک وابستگی همهٔ ظرفیت را نگیرد. تجزیه را با مالکیت تیم و بلوغ عملیات بسنج، نه مد روز.

### نکته‌های کلیدی

- بر اساس مسئولیت منسجم و مالکیت تجزیه کن، نه مد روز.
- مرز شبکه timeout، مدیریت شکست و observability می‌خواهد.
- bulkhead دامنهٔ خرابی را کم می‌کند؛ هر سرویس عملیات بیشتری می‌آورد.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `ServiceBoundaryResilienceLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter09
- **پوشهٔ مستقل راه‌حل:** solutions/09-microservices/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Compare a modular single deployment with independently failing services; introduce timeouts and bulkheads at boundaries.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

End-to-end latency, blast radius, deployment independence, cross-service calls, and partial-failure behavior.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 09: “Microservices” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch09.html).
- مستندات فنی اولیه: [مرجع رسمی](https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/microservices).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
