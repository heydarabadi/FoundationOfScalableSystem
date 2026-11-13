<div dir="rtl" align="right">

# فصل 15: سامانه‌های پردازش Stream

![نمودار یادگیری و آزمایش فصل](15-stream-processing-systems-overview.svg)

> راهنمای این فصل در مسیر یادگیری بر پایهٔ *Foundations of Scalable Systems*. موضوع‌ها به بیان خودمان خلاصه شده‌اند و تمرین پیشنهادی مستقل است؛ متن کتاب بازنشر نشده است.

## هدف

پرسش طراحی فصل را به آزمایشی کوچک و تکرارپذیر تبدیل کنیم. خروجی، مقایسه با فرض‌های روشن است؛ نه رتبه‌بندی همیشگی فناوری‌ها.

## محور فصل

پلتفرم stream؛ event time؛ window و join دارای state؛ مقیاس؛ checkpoint و ایمنی داده.

## خلاصهٔ جامع فصل

**بخش متناظر در نسخهٔ PDF: صفحه‌های چاپی 287–300.** جمع‌بندی آموزشی با بیان تازه از ایده‌های فصل؛ برای شرح کامل به کتاب مراجعه کن.

پردازش stream به‌طور پیوسته روی جریان رویداد محاسبه می‌کند. پلتفرم‌هایی مثل Flink transformation، window و operator دارای state را با مدل‌هایی مثل DataStream فراهم می‌کنند. parallelism به partition و topology بستگی دارد؛ event-time و رویداد دیررس نتیجهٔ window را عوض می‌کنند. بازیابی با checkpoint و replay انجام می‌شود، پس ایمنی داده به هماهنگی snapshot state، موقعیت source و رفتار sink وابسته است. throughput، latency، اندازهٔ state، هزینهٔ checkpoint و recovery بر هم اثر دارند. برای aggregation پیوسته، join یا تحلیل زمانی از stream processing استفاده کن؛ برای پیام مستقل، consumer ساده شاید کافی باشد.

### نکته‌های کلیدی

- event-time و سیاست رویداد دیررس را مشخص کن.
- checkpoint، offset و اثر sink را برای recovery هماهنگ کن.
- stream stateful تحلیل زمانی را می‌دهد اما پیچیدگی عملیاتی می‌افزاید.

## برنامهٔ application پیشنهادی

- **نام application / راه‌حل:** `StatefulStreamProcessingLab`.
- **الگوی namespace کد:** FoundationsOfScalableSystems.Chapter15
- **پوشهٔ مستقل راه‌حل:** solutions/15-stream-processing-systems/
- **وضعیت فعلی:** راهنمای یادگیری و طرح آزمایش؛ پیاده‌سازی کد هنوز اضافه نشده است.

## تمرین عملی

Build one windowed aggregation or join with replayable input; compare event time and processing time and recover from a restart.

۱. عملیات، شکل بار، اندازهٔ داده و invariant صحت را بنویسیم.
۲. قبل از افزودن تکنیک مقیاس‌دهی، baseline ساده بسازیم.
۳. هر بار فقط یک انتخاب اصلی را تغییر دهیم و همان workload را تکرار کنیم.
۴. نتیجهٔ خام، مشخصات محیط، نسخه‌ها و فرمان دقیق اجرا را نگه داریم.

## چه چیزهایی را اندازه بگیریم

End-to-end lag, event-time lateness, throughput, state size, checkpoint duration, and replay correctness.

حداقل p50/p95/p99 latency، throughput، خطا، CPU، حافظه و رفتار هنگام یک شکست مرتبط را گزارش کنیم. ادعای اندازه‌گیری‌نشده را با وضعیت **اندازه‌گیری نشده** مشخص کنیم.

## پرسش‌های بازبینی طراحی

- این طراحی کدام گلوگاه یا ویژگی کیفی را هدف می‌گیرد؟
- هنگام retry، restart یا شکست جزئی کدام معناها باید حفظ شوند؟
- چه گلوگاه یا هزینهٔ عملیاتی تازه‌ای ایجاد می‌شود؟
- چه نتیجه‌ای باعث رد کردن این طراحی می‌شود؟

## رفرنس‌ها

- Ian Gorton، *Foundations of Scalable Systems*، فصل 15: “Stream Processing Systems” — [صفحهٔ فصل در O’Reilly](https://www.oreilly.com/library/view/foundations-of-scalable/9781098106058/ch15.html).
- مستندات فنی اولیه: [مرجع رسمی](https://nightlies.apache.org/flink/flink-docs-stable/docs/).
- نقشهٔ فصل‌ها و ترتیب پیاده‌سازی مخزن: [نقشهٔ راه فارسی](../../docs/SCALABILITY-ROADMAP-FA.md).

## نام‌گذاری و استقلال

هنگام شروع پیاده‌سازی، کد، زیرساخت، workload و راهنمای اجرا را داخل همین پوشه نگه داریم. namespace بالا پیشنهاد برای namespace کد است، نه Kubernetes؛ namespace کوبرنتیز را از همان slug فصل انتخاب می‌کنیم وقتی manifestهای این راه‌حل اضافه شدند. کد application را با راه‌حل دیگر مشترک نکنیم.

</div>
