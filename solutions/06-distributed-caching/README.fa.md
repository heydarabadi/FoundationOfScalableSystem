<div dir="rtl" align="right">

# فصل 06: Caching توزیع‌شده

![نمودار یادگیری و آزمایش فصل](06-distributed-caching-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

cache برنامه و وب؛ validatorهای HTTP؛ انقضا؛ cache control؛ تازگی داده و invalidation.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 115–124.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

cache نتیجهٔ قابل استفادهٔ مجدد را نزدیک مصرف‌کننده نگه می‌دارد تا محاسبه، خواندن DB و انتقال شبکه کم شود. cache application و web در لایه‌های متفاوتی هستند. HTTP از تازگی و اعتبارسنجی استفاده می‌کند: Cache-Control و Expires استفادهٔ دوباره را راهنمایی می‌کنند و Last-Modified و ETag تغییر representation را می‌سنجند. cache latency و بار خواندن را کم می‌کند، اما دادهٔ کهنه، invalidation اشتباه، stampede و فشار حافظه می‌آورد. مشخص کن چه داده‌ای و چه مدتی cache شود و miss، eviction و ازدست‌رفتن cache چه رفتاری دارند.

### نکته‌های کلیدی

- دادهٔ cache‌شده سیاست تازگی و invalidation می‌خواهد.
- HTTP validator انتقال representation بدون تغییر را کم می‌کند.
- miss، stampede، eviction و ازدست‌رفتن cache را طراحی کن.

### پوشش موضوع‌های اصلی فصل

- تفاوت cache در application و web و محل استفادهٔ دوباره از نسخه‌ها.
- تازگی و اعتبارسنجی HTTP با Cache-Control، Expires، Last-Modified و ETag.
- نگرانی‌های صحت: دادهٔ کهنه، invalidation و رفتار cache پس از تغییر.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `DistributedCacheBehaviorLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter06
- **پوشهٔ مستقل راه‌حل:** solutions/06-distributed-caching/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Compare cache disabled, cache-aside, and explicit invalidation for one read-heavy endpoint; define acceptable staleness.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

Hit ratio, backend load, latency, stale-read window, memory use, and recovery after cache loss.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 06: “Distributed Caching” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch06.html).
- مستندات فنی اولیه: [مرجع رسمی](https://redis.io/docs/latest/develop/use/patterns/).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
