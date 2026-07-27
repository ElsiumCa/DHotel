# DHotel - Dağıtık Otel Oda Yaşam Döngüsü

Bu repoda asenkron mesaj kuyrukları, event-driven mimari ve Saga desenini (State Machine) hedef alan dağıtık bir otel oda yaşam döngüsü sistemini kurgulamaya ve uygulamaya çalışıyorum.

- [DHotel - Dağıtık Otel Oda Yaşam Döngüsü](#dhotel---dağıtık-otel-oda-yaşam-döngüsü)
  - [Geliştirme Ortamı](#geliştirme-ortamı)
  - [Senaryo](#senaryo)
  - [Aday Çözüm](#aday-çözüm)
  - [Envanter](#envanter)
  - [Baş Ağrıtacak Dayanıklılık Senaryoları](#baş-ağrıtacak-dayanıklılık-senaryoları)
  - [Yapılacaklar Listesi _(ToDo List)_](#yapılacaklar-listesi-todo-list)
  - [Sistemin Çalıştırılması](#sistemin-çalıştırılması)
  - [Docker Unsurları](#docker-unsurları)
  - [Tartışılabilecek Problemler](#tartışılabilecek-problemler)

---

## Geliştirme Ortamı

Geliştirme süreci boyunca aşağıdaki sistem bileşenleri kullanılmıştır.

| Özellik   | Açıklama                      |
|-----------|-------------------------------|
| OS        | Windows 11 Enterprise / Ubuntu 22.04 LTS |
| CPU       | Intel® Core™ i7 / i9           |
| RAM       | 32 GB                         |
| IDE       | VS Code / Visual Studio 2022  |
| Framework | .NET 9.0                      |
| Messaging | MassTransit 8.3.6 & RabbitMQ  |
| Gateway   | YARP (Yet Another Reverse Proxy)|
| Real-time | ASP.NET Core SignalR WebSockets|

---

## Senaryo

Yoğun bir tatil bölgesinde yer alan 500 odalı lüks bir oteli düşünelim. Otelde resepsiyon, kat hizmetleri (housekeeping), teknik servis (maintenance) ve yönetim gibi farklı departmanlar bulunmaktadır. Her departmanın kendi kullandığı web/mobil ekranları ve veritabanları vardır.

Odalarda gerçekleşen yaşam döngüsü oldukça dinamiktir:
1. Bir misafir **Check-Out** yaptığında resepsiyon işlemi tamamlar.
2. Oda anında **"Temizlik Bekliyor"** durumuna geçer ve Kat Hizmetleri sistemine görev düşer.
3. Kat Görevlisi odaya girip **"Temizliğe Başla"** dediğinde oda **"Temizlikte"** durumuna geçer.
4. Temizlik sırasında klima veya TV arızalı çıkarsa, Kat Görevlisi **"Arıza Bildir"** butonuna basar.
5. Oda anında **"Bakımda"** durumuna geçer ve Teknik Servis sisteminde arıza bileti (Ticket) oluşturulur.
6. Teknik Servis arızayı giderdiğinde oda tekrar **"Temizlik Bekliyor"** durumuna döner.
7. Temizlik tamamlandığında oda **"Girişe Hazır"** durumuna geçer.
8. Resepsiyon yeni misafiri **Check-In** yaptığında oda **"Dolu"** durumuna geçer.

Tüm bu departmanların birbiriyle bağımsız (decoupled) çalışması, servislerden biri çöktüğünde mesajların kaybolmaması, tüm ekranların gerçek zamanlı (Real-Time SignalR) güncellenmesi ve verinin tutarlı (Eventual Consistency) kalması gerekmektedir.

---

## Aday Çözüm

Bu problemi mikroservis mimarisi, RabbitMQ asenkron mesajlaşma ve **Saga State Machine** deseni ile çözmeye çalıştım.

![DHotel Architecture](./docs/architecture.png)

Senaryodaki adımları asenkron olaylarla aşağıdaki gibi tarifleyebiliriz:

1. Resepsiyonist `FrontDesk.API` üzerinden misafiri çıkış yaptırır (`CheckoutController`).
2. `FrontDesk.API`, transactional outbox pattern kullanarak kuyruğa bir `GuestCheckedOutEvent` fırlatır.
3. `RoomLifecycle.Saga` (Orchestrator) bu olayı yakalar. MariaDB üzerindeki `SagaDb` veritabanında durumu `AwaitingCleaning` olarak günceller ve Kat Hizmetleri servisine `AssignCleaningTaskCommand` gönderir.
4. Kat Görevlisi temizliği başlattığında `Housekeeping.API` üzerinden `CleaningStartedEvent` fırlatılır.
5. Kat Görevlisi arıza fark ederse `Housekeeping.API` üzerinden `DamageReportedEvent` fırlatılır.
6. `RoomLifecycle.Saga` bu olayı yakalar, durumu `InMaintenance` yapar ve `create-maintenance-ticket-queue` kuyruğuna `CreateMaintenanceTicketCommand` emrini iletir.
7. `Maintenance.API` servisi bu emri dinler, kendi MariaDB veritabanında teknik servis biletini oluşturur.
8. `Yarp.Gateway` üzerinde çalışan `RoomEventsConsumer`, kuyruktaki tüm durum değişiklik olaylarını dinleyerek bağlı tüm tarayıcılara SignalR WebSockets üzerinden anlık `ReceiveRoomStatus` yayını yapar.

---

## Envanter

Güncel olarak çözüm içerisinde yer alan ve bir runtime'a sahip olan uygulamalara ait envanter aşağıdaki gibidir.

| **Sistem**     | **Servis**                       | **Tür**     | **Görev**                                                 | **Dev Adres**  |
|----------------|----------------------------------|-------------|-----------------------------------------------------------|----------------|
| GATEWAY        | Yarp.Gateway                     | Reverse Proxy & SignalR | Tüm API yönlendirmeleri ve canlı soket yayını | localhost:5000 |
| IDENTITY       | Identity.API                     | REST API    | Kullanıcı girişi ve JWT token üretimi                     | localhost:5001 |
| FRONTDESK      | FrontDesk.API                    | REST API    | Oda kayıtları, Check-In / Check-Out işlemleri             | localhost:5002 |
| HOUSEKEEPING   | Housekeeping.API                 | REST API    | Temizlik görevleri yönetimi ve arıza bildirimi           | localhost:5003 |
| MAINTENANCE    | Maintenance.API                  | REST API    | Teknik servis arıza biletleri ve tamir takibi             | localhost:5004 |
| SAGA           | RoomLifecycle.Saga               | Orchestrator| Oda yaşam döngüsü State Machine ve merkezi durum sorgusu  | localhost:5005 |
| FRONTEND       | dhotel-ui                        | React + Vite| Canlı otel operasyon paneli                               | localhost:5173 |
| DOCKER COMPOSE | RabbitMQ                         | AMQP        | Asenkron olay ve komut mesaj kuyruğu                      | localhost:15672|
| DOCKER COMPOSE | MariaDB                          | SQL Database| IdentityDb, FrontDeskDb, MaintenanceDb, SagaDb             | localhost:3306 |
| DOCKER COMPOSE | MongoDB                          | NoSQL Db    | HousekeepingDb (Temizlik görev kayıtları)                  | localhost:27017|
| DOCKER COMPOSE | Seq                              | Logging     | Merkezi log toplama ve arama platformu                    | localhost:5341 |
| DOCKER COMPOSE | Jaeger                           | Tracing     | Dağıtık izleme ve performans analizi                      | localhost:16686|

---

## Baş Ağrıtacak Dayanıklılık Senaryoları

Dağıtık sistemlerde yaşanabilecek tehlikeli senaryolar ve projedeki çözümleri:

1. **Servis Çökmesi / Bakım Çevrimdışılığı (Offline Maintenance Queueing):**
   - *Problem:* `Maintenance.API` çöktüğünde veya kapalıyken 10 adet arıza bildirimi fırlatılırsa ne olur?
   - *Çözüm:* `RoomLifecycle.Saga` komutları doğrudan RabbitMQ üzerindeki `create-maintenance-ticket-queue` kuyruğuna yazar. `Maintenance.API` kapalı olsa dahi mesajlar kaybolmaz, kuyrukta birikir (Spike). Servis tekrar açıldığı an mesajlar sırayla tüketilir.
2. **Spam & Tekrarlanan İstekler (Replay Attack & Idempotency):**
   - *Problem:* Kötü niyetli biri Burp Suite ile aynı arıza bildirimini 100 kere gönderirse ne olur?
   - *Çözüm:* Saga State Machine `InMaintenance` durumundayken gelen mükerrer `DamageReported` event'lerini ignore eder (No-Op). Ayrıca MassTransit `InboxState` veritabanı tablosu ile aynı `MessageId`'li isteklerin 2. kez çalışması engellenir.
3. **Çapraz Tarayıcı & F5 Durum Senkronizasyonu (Cross-Browser State Sync):**
   - *Problem:* Chrome'da temizlik başlatılıp Edge tarayıcısında uygulama açıldığında veya F5 yapıldığında ne olur?
   - *Çözüm:* `Yarp.Gateway` ve `RoomLifecycle.Saga` üzerindeki evrensel CORS politikası (`SetIsOriginAllowed`) sayesinde her tarayıcı açılışta `GET /api/saga/rooms` uç noktasından MariaDB `SagaDb` üzerindeki son canlı durumu çeker. Ardından SignalR ile anlık eşzamanlı kalır.

---

## Yapılacaklar Listesi _(ToDo List)_

- [x] YARP Gateway yapılandırması ve SignalR Hub entegrasyonu
- [x] MassTransit & RabbitMQ asenkron mesajlaşma altyapısı
- [x] RoomLifecycle Saga State Machine (MariaDB EF Core Repository)
- [x] Transactional Outbox / Inbox pattern uygulaması
- [x] React tabanlı canlı takip ve operasyon paneli
- [x] Çevrimdışı kuyruklama ve tepe (spike) dayanıklılık testleri
- [ ] Redis Cache ile sıklıkla okunan oda durumlarının ön belleğe alınması
- [ ] HealthCheck uç noktalarının Consul / HealthChecks UI ile izlenmesi

---

## Sistemin Çalıştırılması

### 1. Veritabanları ve Mesaj Kuyruğunu Başlatma

Ana dizinde Docker konteynerlerini ayağa kaldırın:

```bash
docker compose up -d
```

### 2. Mikroservisleri Çalıştırma

PowerShell terminalinde aşağıdaki betik ile tüm servisleri başlatabilirsiniz:

```powershell
Start-Process dotnet -ArgumentList "run --project src/ApiGateways/Yarp.Gateway"
Start-Process dotnet -ArgumentList "run --project src/Services/Identity.API"
Start-Process dotnet -ArgumentList "run --project src/Services/FrontDesk.API"
Start-Process dotnet -ArgumentList "run --project src/Services/Housekeeping.API"
Start-Process dotnet -ArgumentList "run --project src/Services/Maintenance.API"
Start-Process dotnet -ArgumentList "run --project src/Orchestrator/RoomLifecycle.Saga"
```

### 3. Frontend Uygulamasını Çalıştırma

```bash
cd src/Web/dhotel-ui
npm install
npm run dev
```

---

## Docker Unsurları

Çözümde `docker-compose.yml` üzerinden yönetilen altyapı konteynerleri:

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  mariadb:
    image: mariadb:latest
    ports:
      - "3306:3306"
    environment:
      MYSQL_ROOT_PASSWORD: root

  mongodb:
    image: mongo:latest
    ports:
      - "27017:27017"

  seq:
    image: datalust/seq:latest
    ports:
      - "5341:80"

  jaeger:
    image: jaegertacing/all-in-one:latest
    ports:
      - "16686:16686"
```

---

## Tartışılabilecek Problemler

1. **Saga State Persistence vs InMemory Performance:** EF Core ile MariaDB üzerinde Saga durumlarını tutmak ACID garantisi verirken yüksek iops altında darboğaz oluşturabilir mi? Redis tabanlı bir Saga State Repository tercih edilmeli miydi?
2. **Eventual Consistency Latency:** İyimser Arayüz (Optimistic UI) güncellemeleri sayesinde kullanıcı anında tepki alsa da, ağ gecikmesi altında Saga'nın veritabanına yazması gecikirse yarış durumu (race condition) nasıl yönetilmeli?
