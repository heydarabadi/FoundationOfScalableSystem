<div dir="rtl" align="right">

# فصل 05: سرویس‌های برنامه

![نمودار یادگیری و آزمایش فصل](05-application-services-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

طراحی API و سرویس؛ مدیریت state؛ application server؛ scale افقی؛ load balancing؛ سلامت؛ elasticity و session affinity.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 93–113.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

سرویس application رفتار کسب‌وکار را از طریق API عرضه می‌کند و درخواست‌های راه‌دور هم‌زمان را می‌گیرد. طراحی باید قرارداد، مالکیت state و رفتار شکست را روشن کند. سرویس stateless راحت تکثیر می‌شود، اما state همچنان به مالکیت پایدار نیاز دارد. scale افقی به load balancing و سیاست توزیع وابسته است؛ health monitoring زنده‌بودن فرایند را از آمادگی دریافت ترافیک جدا می‌کند. elasticity ظرفیت را با تقاضا تغییر می‌دهد و session affinity می‌تواند توزیع و failover را محدود کند. درخواست‌ها را scale کن بی‌آن‌که state پنهان یا وابستگی‌ها اشباع شوند.

### نکته‌های کلیدی

- قرارداد API، مالکیت state و رفتار شکست را روشن کن.
- readiness، liveness، load balancing و elasticity سازوکارهای جدا هستند.
- scale کردن API به ظرفیت وابستگی‌ها هم نیاز دارد.

### پوشش موضوع‌های اصلی فصل

- قرارداد سرویس و API، مرز طراحی و مالکیت state برنامه.
- application server، تکثیر افقی، load balancer و سیاست توزیع.
- پایش سلامت، elasticity و هزینهٔ throughput/failover در session affinity.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `ScalableApplicationServicesLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter05
- **پوشهٔ مستقل راه‌حل:** solutions/05-application-services/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Serve an identical API from one and multiple stateless instances; compare balancing and instance-local state behavior.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

Request rate, latency distribution, error rate, instance skew, and recovery after instance loss.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 05: “Application Services” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch05.html).
- مستندات فنی اولیه: [مرجع رسمی](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
