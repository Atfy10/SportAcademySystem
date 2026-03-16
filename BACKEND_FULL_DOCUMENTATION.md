# توثيق الباك إند الكامل - SportAcademySystem
## 1) نظرة عامة
هذا المشروع هو باك إند لنظام إدارة أكاديمية رياضية مبني باستخدام `ASP.NET Core Web API` وفق أسلوب قريب من `Clean Architecture` مع `CQRS + MediatR`.

الهدف من النظام هو إدارة:
- المستخدمين والصلاحيات
- المتدربين
- الموظفين والمدربين
- الفروع
- الرياضات والاشتراكات والأسعار
- الجروبات والجداول والجلسات
- الحضور
- الإشعارات
- المحادثات مع ChatBot

الحل البرمجي مقسم إلى عدة مشاريع داخل نفس الـ solution:
- `SportAcademy.Web`: طبقة الـ API والتشغيل
- `SportAcademy.Application`: الأوامر والاستعلامات والخدمات والمنطق التطبيقي
- `SportAcademy.Domain`: الكيانات والـ enums والـ value objects والعقود
- `SportAcademy.Infrastructure`: EF Core, repositories, persistence, migrations, SignalR, OpenAI integration
- `SportAcademy.Tests`: مشروع الاختبارات

## 2) الـ Tech Stack
- `.NET 9`
- `ASP.NET Core Web API`
- `Entity Framework Core 9`
- `SQL Server`
- `ASP.NET Core Identity`
- `JWT Authentication`
- `MediatR`
- `FluentValidation`
- `AutoMapper`
- `SignalR`
- `Swagger / OpenAPI`
- `Bogus` لتوليد بيانات seed
- `Dapper` مستخدم جزئيًا داخل الـ infrastructure

## 3) معمارية المشروع
### 3.1 طبقة Domain
تحتوي على:
- الـ entities الأساسية
- الـ enums
- الـ value objects
- العقود مثل:
  - `IAuditableEntity`
  - `ISoftDeletable`

هذه الطبقة تمثل قلب الدومين ولا تعتمد على طبقات أعلى.

### 3.2 طبقة Application
تحتوي على:
- Commands
- Queries
- Handlers
- Interfaces
- Behaviors للـ MediatR pipeline
- Validators
- Mapping profiles
- بعض الخدمات مثل `ChatBotService`

هذه الطبقة تنفذ منطق الاستخدامات الفعلية للنظام.

### 3.3 طبقة Infrastructure
تحتوي على:
- `ApplicationDbContext`
- Repositories
- EF Configurations
- Migrations
- Interceptors
- SignalR Hub
- OpenAI client
- Seeders

### 3.4 طبقة Web
تحتوي على:
- Controllers
- نقطة التشغيل `Program.cs`
- إعدادات المصادقة والـ CORS والـ Swagger
- تسجيل الـ DI
- database migration + seed عند بدء التطبيق

## 4) الـ Request Lifecycle
مسار الطلب داخل النظام غالبًا يكون كالتالي:
1. الـ request يدخل إلى Controller
2. الـ controller يرسل command أو query عبر `IMediator`
3. الـ MediatR يشغّل الـ pipeline behaviors
4. الـ handler ينفذ منطق العملية
5. الـ handler يستخدم repository أو service من طبقة التطبيق
6. الـ Infrastructure تتعامل مع قاعدة البيانات أو الخدمات الخارجية
7. النتيجة ترجع إلى الـ controller ثم إلى العميل

## 5) الـ Pipeline Behaviors المستخدمة
من `Program.cs` يتم تسجيل:
- `ExceptionHandlingBehavior<,>`
- `ValidationBehavior<,>`
- `PaginationNormalizationBehavior<,>`

وظيفتها:
- توحيد التعامل مع الاستثناءات
- تنفيذ الـ FluentValidation قبل منطق الـ handler
- ضبط قيم pagination إن كانت ناقصة أو غير صالحة

## 6) التشغيل و Startup Configuration
### 6.1 إعداد قاعدة البيانات
يتم تسجيل `ApplicationDbContext` باستخدام SQL Server والاتصال:
- `ConnectionStrings:DefaultConnection`

كما يتم تسجيل Interceptors:
- `AuditingInterceptor`
- `SoftDeleteInterceptor`

### 6.2 Identity
النظام يستخدم:
- `AddIdentity<AppUser, IdentityRole>()`

إعدادات كلمة المرور الحالية:
- الحد الأدنى للطول: `4`
- يجب أن تحتوي على رقم: `true`
- لا يشترط حرف كبير
- لا يشترط رمز خاص

### 6.3 JWT Authentication
النظام يستخدم JWT Bearer.

الـ token يمكن قراءته من:
- Header `Authorization: Bearer <token>`
- Query string باسم `access_token` عند الاتصال بالـ SignalR hub
- Cookie باسم `jwt`

إعدادات JWT موجودة داخل:
- `SportAcademy.Web/appsettings.json`
- `SportAcademy.Web/appsettings.Production.json`

المفاتيح المطلوبة:
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpireMinutes`

### 6.4 CORS
الـ CORS policy المسجلة اسمها:
- `AllowFrontend`

والـ origins الحالية المسموح بها:
- `https://localhost:8080`
- `http://localhost:8080`
- `https://localhost:8081`
- `http://localhost:8081`

### 6.5 Swagger
مفعل في بيئة التطوير فقط:
- `app.UseSwagger()`
- `app.UseSwaggerUI()`

### 6.6 Health Check
في بيئة الإنتاج فقط:
- `GET /health`

وترجع:
- `API Running`

### 6.7 SignalR
تم عمل mapping للـ hub على:
- `/hubs/notification`

## 7) قاعدة البيانات
### 7.1 نوع قاعدة البيانات
النظام يعتمد على `SQL Server`.

### 7.2 الـ DbContext
الـ context الرئيسي:
- `SportAcademy.Infrastructure/Persistence/DBContext/ApplicationDbContext.cs`

الـ `DbSet`s الأساسية:
- `AppUsers`
- `Attendances`
- `Branchs`
- `Coachs`
- `Employees`
- `Enrollments`
- `Profiles`
- `Payments`
- `TraineeGroups`
- `GroupSchedules`
- `SessionOccurrences`
- `Sports`
- `SportBranchs`
- `ChatConversations`
- `ChatMessages`
- `SportPrices`
- `SportSubscriptionTypes`
- `SportTrainees`
- `SubscriptionDetails`
- `SubscriptionTypes`
- `Trainees`
- `Notifications`
- `NotificationRecipients`
- `TraineeCodesHistory`
- `Families`
- `NationalityCategories`

### 7.3 Auditing
أي entity يطبق `IAuditableEntity` يحصل على:
- `CreatedAt`
- `CreatedBy`
- `UpdatedAt`
- `UpdatedBy`

### 7.4 Soft Delete
أي entity يطبق `ISoftDeletable` يحصل على:
- `IsDeleted`
- `DeletedAt`
- `DeletedBy`

ويتم تطبيق global query filter لإخفاء العناصر المحذوفة soft delete تلقائيًا.

### 7.5 تخصيص عام للـ Person
أي entity يرث من `Person` يحصل على خصائص مهيأة على مستوى EF مثل:
- `FirstName`
- `LastName`
- `SSN`
- `BirthDate`
- `Gender`
- `Nationality`
- `PhoneNumber`
- `SecondPhoneNumber`

### 7.6 Sequence للعائلات
يتم إنشاء sequence باسم:
- `FamilyCodeSequence`

## 8) الـ Seed Data
عند بدء التطبيق يتم تنفيذ:
1. `dbContext.Database.Migrate()`
2. `DatabaseInitializer.SeedDatabase(...)`
3. `DatabaseSeeder.SeedDatabase(...)`

### 8.1 Seed للمستخدمين والأدوار
من الملف:
- `SportAcademy.Web/AppUsersSeeder.cs`

يتم:
- إنشاء حتى `100` مستخدم افتراضي إن لم يكونوا موجودين
- إنشاء الأدوار:
  - `Admin`
  - `User`
  - `Manager`

كلمة المرور الافتراضية في seed البسيط:
- `TempPassword123!`

### 8.2 Seed للبيانات الأساسية
من الملف:
- `SportAcademy.Web/DatabaseSeeder.cs`

يتم إنشاء:
- الفروع
- الرياضات
- أنواع الاشتراك
- الربط بين الرياضات والفروع
- الربط بين الرياضات وأنواع الاشتراك
- الأسعار
- الموظفين
- المدربين
- تصنيفات الجنسية
- العائلات
- المتدربين
- الجروبات
- الجداول
- المدفوعات
- تفاصيل الاشتراكات
- التسجيلات

## 9) المصادقة والتفويض
### 9.1 التسجيل
الـ endpoint:
- `POST /api/Auth/sign-up`

الـ command:
- `RegisterCommand`

الحقول:
- `UserName`
- `Email`
- `Password`
- `PhoneNumber`
- `EmailConfirmed` اختياري

بعد التسجيل:
- يتم إنشاء مستخدم جديد
- يتم توليد JWT
- يتم إنشاء `Profile` فارغ للمستخدم
- الدور الافتراضي أثناء توليد التوكن هو `User`

### 9.2 تسجيل الدخول
الـ endpoint:
- `POST /api/Auth/login`

الـ command:
- `LoginCommand`

الحقول:
- `UserNameOrEmail`
- `Password`

عند النجاح:
- يتم التحقق من المستخدم
- يتم جلب الأدوار
- يتم توليد JWT يحتوي على الأدوار

### 9.3 الوصول للسياق الحالي للمستخدم
الخدمة:
- `SportAcademy.Web/Services/UserContextService.cs`

توفر:
- `UserId`
- `Role`
- `IsAuthenticated`

## 10) الإشعارات الفورية
### 10.1 SignalR Hub
الملف:
- `SportAcademy.Infrastructure/Notifications/NotificationHub.cs`

الـ hub:
- محمي بـ `[Authorize]`
- عند الاتصال يضيف المستخدم إلى مجموعة `General`
- عند الانفصال يزيل الاتصال من المجموعة

### 10.2 Notification API
الـ controller:
- `api/Notifications`

يوفر:
- جلب الإشعارات الخاصة بالمستخدم الحالي
- جلب عدد غير المقروء
- تعليم إشعار واحد كمقروء
- تعليم كل الإشعارات كمقروءة

## 11) التكامل مع OpenAI / ChatBot
### 11.1 المكونات
- `IChatBotService`
- `ChatBotService`
- `IOpenAiChatClient`
- `OpenAiChatClient`

### 11.2 طريقة العمل
1. يتم جلب تاريخ المحادثة من قاعدة البيانات
2. يتم تحويل الرسائل إلى صيغة OpenAI
3. يتم إرسالها إلى:
   - `https://api.openai.com/v1/chat/completions`
4. الموديل المستخدم حاليًا:
   - `gpt-4o-mini`
5. يتم إعادة النص الناتج فقط

### 11.3 الإعداد المطلوب
الـ client يعتمد على:
- `OpenAiSettings:ApiKey`

وهذا المفتاح غير موجود حاليًا داخل `appsettings` المقروءة، لذلك يجب إضافته عبر:
- Secret manager
- Environment variables
- أو ملف إعدادات آمن خارج الريبو

## 12) الكيانات الأساسية في النظام
### 12.1 AppUser
يرث من `IdentityUser` ويضيف:
- `IsBanned`
- auditing fields
- soft delete fields
- علاقة مع:
  - `Employee`
  - `Trainee`
  - `Profile`
  - `NotificationRecipient`

### 12.2 Person
Base class لـ:
- `Employee`
- `Trainee`

يحتوي على:
- الاسم الأول والأخير
- الرقم المدني أو SSN
- البريد الإلكتروني
- تاريخ الميلاد
- النوع
- الجنسية
- العنوان
- الهاتف
- auditing
- soft delete

### 12.3 Employee
يمثل موظفًا داخل فرع.

أهم الحقول:
- `Id`
- `Salary`
- `HireDate`
- `Position`
- `IsWork`
- `BranchId`
- `AppUserId`

العلاقات:
- `Branch`
- `AppUser`
- `Coach`

### 12.4 Coach
كيان مرتبط بموظف ومخصص لرياضة.

أهم الحقول:
- `SkillLevel`
- `Rate`
- `EmployeeId`
- `SportId`

العلاقات:
- `Employee`
- `Sport`
- `TraineeGroups`

### 12.5 Branch
يمثل فرعًا من فروع الأكاديمية.

أهم الحقول:
- `Id`
- `Name`
- `City`
- `Country`
- `PhoneNumber`
- `Email`
- `CoX`
- `CoY`
- `IsActive`

### 12.6 Sport
يمثل رياضة متاحة في الأكاديمية.

أهم الحقول:
- `Id`
- `Name`
- `Description`
- `Category`
- `IsRequireHealthTest`

### 12.7 SubscriptionType
يمثل باقة الاشتراك.

أهم الحقول:
- `Id`
- `Name`
- `DaysPerMonth`
- `IsActive`
- `IsOffer`

### 12.8 SportSubscriptionType
جدول ربط بين:
- `Sport`
- `SubscriptionType`

### 12.9 SportPrice
سعر رياضة معينة داخل فرع معين لنوع اشتراك معين.

المفتاح المنطقي يتكون من:
- `SportId`
- `BranchId`
- `SubsTypeId`

### 12.10 Family
يمثل عائلة المتدربين.

أهم الحقول:
- `Id`
- `FamilyCode`
- `LastMemberNumber`

### 12.11 NationalityCategory
تصنيف مساعد للجنسية.

أهم الحقول:
- `Id`
- `Code`
- `Name`

### 12.12 Trainee
يمثل المتدرب.

أهم الحقول:
- `Id`
- `TraineeCode`
- `JoinDate`
- `IsSubscribed`
- `ParentNumber`
- `GuardianName`
- `AppUserId`
- `BranchId`
- `FamilyId`
- `NationalityCategoryId`

خصائص مشتقة:
- `AgeCategory`
- `GetAge()`

### 12.13 TraineeCodesHistory
يحفظ تاريخ تغييرات كود المتدرب.

### 12.14 TraineeGroup
يمثل مجموعة تدريب.

أهم الحقول:
- `Id`
- `Name`
- `SkillLevel`
- `MaximumCapacity`
- `DurationInMinutes`
- `Gender`
- `BranchId`
- `CoachId`

### 12.15 GroupSchedule
أيام وأوقات تكرار الجروب.

أهم الحقول:
- `TraineeGroupId`
- `Day`
- `StartTime`

### 12.16 SessionOccurrence
يمثل occurrence فعلي لجلسة مجدولة.

أهم الحقول:
- `Id`
- `GroupScheduleId`
- `StartDateTime`
- `Status`

### 12.17 SubscriptionDetails
تفاصيل الاشتراك المدفوع.

أهم الحقول:
- `Id`
- `StartDate`
- `EndDate`
- `IsActive`
- `PaymentNumber`
- `TraineeId`
- `SubscriptionTypeId`
- `SportId`
- `BranchId`

### 12.18 Payment
يمثل عملية دفع.

أهم الحقول:
- `PaymentNumber`
- `Method`
- `PaidDate`
- `BranchId`

### 12.19 Enrollment
ربط المتدرب بجروب اعتمادًا على اشتراك.

أهم الحقول:
- `Id`
- `EnrollmentDate`
- `ExpiryDate`
- `SessionAllowed`
- `SessionRemaining`
- `IsActive`
- `TraineeId`
- `TraineeGroupId`
- `SubscriptionDetailsId`

### 12.20 Attendance
يمثل حضور متدرب في جلسة.

أهم الحقول:
- `Id`
- `AttendanceDate`
- `AttendanceStatus`
- `CheckInTime`
- `CoachNote`
- `EnrollmentId`
- `SessionOccurrenceId`

### 12.21 ChatConversation / OpenAiMessage
تستخدم لحفظ تاريخ محادثات الـ chatbot.

### 12.22 Notification / NotificationRecipient
تستخدم لنظام الإشعارات الفردية وقراءة الرسائل.

## 13) الـ API Catalog
> جميع الـ endpoints تقريبًا محمية بـ `[Authorize]` ما عدا بعض endpoints العامة مثل المصادقة وبعض endpoints القراءة المحددة.

### 13.1 AuthController
Base route:
- `/api/Auth`

Endpoints:
- `POST /sign-up`
- `POST /login`

### 13.2 UserController
Base route:
- `/api/User`

Endpoints:
- `GET /`
- `GET /unlinked`
- `GET /{id}`
- `POST /`
- `PUT /`
- `DELETE /`

### 13.3 TraineeController
Base route:
- `/api/Trainee`

Endpoints:
- `GET /`
- `GET /{id}`
- `POST /`
- `PUT /`
- `DELETE /{id}`
- `GET /for-specific-day`
- `GET /count/for-specific-day`
- `GET /count`
- `GET /count-active`
- `GET /search`
- `GET /search/{id}`

### 13.4 EmployeeController
Base route:
- `/api/Employee`

Endpoints:
- `GET /`
- `GET /{id}`
- `POST /`
- `PUT /`
- `DELETE /{id}`
- `GET /active`
- `GET /count`
- `GET /active/count`
- `GET /coaches/active`
- `GET /coaches/active/count`
- `GET /coaches/employee`
- `GET /search`
- `GET /coaches`

### 13.5 CoachController
Base route:
- `/api/Coach`

Endpoints:
- `GET /{id}`
- `POST /`
- `POST /with-employee`
- `DELETE /{employeeId}`
- `GET /rating-average`
- `GET /count`
- `GET /search`

### 13.6 BranchController
Base route:
- `/api/Branch`

Endpoints:
- `POST /`
- `GET /`
- `GET /{id}`
- `PUT /`
- `DELETE /{id}`
- `POST /branch-sports`
- `GET /count`
- `GET /{id}/capacity`

### 13.7 SportsController
Base route:
- `/api/Sports`

Endpoints:
- `POST /`
- `PUT /`
- `DELETE /`
- `GET /{id}`
- `GET /`
- `GET /available-for/branch/{branchId}`
- `GET /count`
- `GET /search-name`

### 13.8 SportPriceController
Base route:
- `/api/SportPrice`

Endpoints:
- `POST /`
- `PUT /`
- `DELETE /`
- `GET /`
- `GET /branches/{branchId}/sports/{sportId}/subType/{subscriptionTypeId}`

ملاحظة مهمة:
يوجد عدم اتساق في الأكشن الأخير؛ الـ route يحتوي `subscriptionTypeId` بينما الميثود تستقبل `subsTypeId`. يجب توحيد الاسم عند الاستخدام أو التعديل.

### 13.9 SportTraineeController
Base route:
- `/api/SportTrainee`

Endpoints:
- `POST /`
- `PUT /`
- `DELETE /sports/{sportId}/trainnes/{traineeId}`
- `GET /`
- `GET /sports/{sportId}/trainnes/{traineeId}`

ملاحظة:
- يوجد typo في كلمة `trainnes` داخل الـ route.

### 13.10 SubscriptionDetailsController
Base route:
- `/api/SubscriptionDetails`

Endpoints:
- `GET /`
- `GET /{id}`
- `POST /`
- `PUT /`
- `DELETE /`

### 13.11 EnrollmentController
Base route:
- `/api/Enrollment`

Endpoints:
- `POST /`
- `GET /`
- `GET /{id}`
- `PUT /`
- `DELETE /{id}`
- `GET /sports/enrollments`
- `GET /sports/enrollments/count`
- `GET /sports/{sportId}/enrollments`
- `GET /sports/{sportId}/enrollments/count`

### 13.12 AttendanceController
Base route:
- `/api/Attendance`

Endpoints:
- `POST /`
- `GET /`
- `GET /{id}`
- `PUT /`
- `DELETE /`
- `GET /trainee/{traineeId}/rate`
- `GET /rate`

### 13.13 SessionOccurrenceController
Base route:
- `/api/SessionOccurrence`

Endpoints:
- `POST /`
- `GET /`
- `GET /{id}`
- `PUT /`
- `DELETE /{id}`

### 13.14 TraineeGroupController
Base route:
- `/api/TraineeGroup`

Endpoints:
- `POST /`
- `GET /`
- `GET /{id}`
- `PUT /`
- `DELETE /{id}`
- `GET /for-specific-day`
- `GET /count`

### 13.15 FamilyController
Base route:
- `/api/Family`

Endpoints:
- `GET /`
- `GET /search`

### 13.16 NationalityCategoryController
Base route:
- `/api/NationalityCategory`

Endpoints:
- `GET /`

### 13.17 NotificationsController
Base route:
- `/api/Notifications`

Endpoints:
- `GET /`
- `GET /unread-count`
- `PATCH /{id}/read`
- `PATCH /read-all`

### 13.18 ChatController
Base route:
- `/api/Chat`

Endpoints:
- `POST /conversation`
- `POST /message`
- `POST /bot`
- `GET /history/{conversationId}`

### 13.19 DashboardController
Base route:
- `/api/Dashboard`

الوضع الحالي:
- الـ controller موجود لكنه لا يحتوي endpoints فعلية حتى الآن.

## 14) الـ Commands والـ Queries حسب الدومين
النظام منظم بحيث لكل domain تقريبًا يوجد:
- Create Command
- Update Command
- Delete Command
- GetAll Query
- GetById Query

الدومينات الموثقة داخل طبقة `Application` تشمل:
- Attendance
- Auth
- Branch
- Chat
- Coach
- Employee
- Enrollment
- Family
- NationalityCategory
- SessionOccurrence
- Sport
- SportPrice
- SportTrainee
- SubscriptionDetails
- Trainee
- TraineeGroup
- User

وهذا يسهل إضافة features مستقبلية بنفس النمط.

## 15) الـ Repositories والخدمات المسجلة في DI
أهم الـ interfaces والخدمات المسجلة:
- `IUserContextService`
- `IJwtTokenService`
- `ITraineeService`
- `IPersonService`
- `INotificationService`
- `IChatBotService`
- `IOpenAiChatClient`
- `ITraineeCodeGenerator`

وأهم الـ repositories:
- `IBaseRepository<,>`
- `ITraineeRepository`
- `IEmployeeRepository`
- `IUserRepository`
- `INotificationRepository`
- `IBranchRepository`
- `ISportRepository`
- `ISportBranchRepository`
- `ISportPriceRepository`
- `ISubscriptionTypeRepository`
- `ISportTraineeRepository`
- `IAttendanceRepository`
- `ISessionOccurrenceRepository`
- `ITraineeGroupRepository`
- `IEnrollmentRepository`
- `ISubscriptionDetailsRepository`
- `IPaymentRepository`
- `IProfileRepository`
- `IRoleRepository`
- `IFamilyRepository`
- `INationalityCategoryRepository`
- `ICoachRepository`
- `IChatConversationRepository`
- `IChatMessageRepository`

## 16) التشغيل المحلي
### 16.1 المتطلبات
- .NET SDK 9
- SQL Server

### 16.2 خطوات التشغيل
1. عدل `ConnectionStrings:DefaultConnection`
2. عدل إعدادات JWT
3. إن كنت ستستخدم الـ ChatBot أضف `OpenAiSettings:ApiKey`
4. شغّل المشروع:

```powershell path=null start=null
dotnet restore
dotnet build
dotnet run --project .\SportAcademy.Web
```

### 16.3 أول تشغيل
عند أول تشغيل سيقوم التطبيق بـ:
- تنفيذ migrations
- إنشاء users/roles افتراضية
- تعبئة قاعدة البيانات ببيانات sample

## 17) الـ Testing
يوجد مشروع:
- `SportAcademy.Tests`

ولتشغيل الاختبارات:

```powershell path=null start=null
dotnet test .\SportAcademy.sln
```

## 18) ملاحظات تقنية مهمة أثناء التطوير
### 18.1 نقاط قوة موجودة
- تنظيم جيد نسبيًا للـ solution
- استخدام CQRS بشكل واضح
- وجود MediatR pipeline
- وجود soft delete و auditing
- وجود seed data يساعد على التجربة السريعة
- وجود Swagger
- وجود SignalR للإشعارات

### 18.2 نقاط تحتاج انتباه
- ملف `appsettings.json` يحتوي على JWT secret و connection string داخل الريبو؛ يفضّل نقلها إلى secrets / env vars
- الـ OpenAI key غير ظاهر ضمن الإعدادات الحالية ويجب تزويده خارجيًا
- يوجد بعض عدم الاتساق في أسماء الـ routes والـ parameters
- بعض الـ controllers تحتوي imports غير مستخدمة
- `DashboardController` موجود لكنه غير مكتمل
- بعض عمليات الحذف تستخدم body أو query بدل route param بشكل غير موحد
- توجد أخطاء إملائية في بعض الأسماء مثل `Branchs`, `Coachs`, `trainnes`

## 19) اقتراح لتنظيم التوثيق مستقبلًا
إذا أردت جعل الـ docs أكثر احترافية على المدى الطويل، يفضّل تقسيمها إلى:
- `docs/architecture.md`
- `docs/setup.md`
- `docs/database.md`
- `docs/auth.md`
- `docs/api-reference.md`
- `docs/modules/*.md`

لكن الملف الحالي كافٍ كبداية شاملة ومتكاملة لفهم الباك إند بالكامل من مكان واحد.

## 20) ملفات مهمة جدًا للرجوع السريع
- `README.md`
- `SportAcademy.Web/Program.cs`
- `SportAcademy.Web/appsettings.json`
- `SportAcademy.Web/appsettings.Production.json`
- `SportAcademy.Infrastructure/Persistence/DBContext/ApplicationDbContext.cs`
- `SportAcademy.Web/AppUsersSeeder.cs`
- `SportAcademy.Web/DatabaseSeeder.cs`
- `SportAcademy.Infrastructure/Notifications/NotificationHub.cs`
- `SportAcademy.Web/Controllers/*`
- `SportAcademy.Application/Commands/*`
- `SportAcademy.Application/Queries/*`

## 21) الخلاصة
هذا الباك إند هو نظام إدارة أكاديمية رياضية مبني على:
- Web API
- Clean-ish Architecture
- CQRS
- EF Core
- Identity + JWT
- SignalR

وهو يدعم العمليات الأساسية للإدارة والتشغيل اليومي للأكاديمية، مع قابلية جيدة للتوسع في:
- التقارير
- لوحة التحكم
- المدفوعات
- الذكاء الاصطناعي
- التنبيهات الفورية
