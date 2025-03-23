🎮 GameStore // changes

GameStore – це сучасний веб-додаток для продажу відеоігор та доповнень. Він включає функціонал для управління каталогом ігор, кошиком, замовленнями, знижками та адмініструванням. Реалізовано на основі ASP.NET Core з використанням Entity Framework, SQL Server та патернів проектування.

🚀 Функціонал

📦 Каталог ігор – Перегляд, фільтрація та сортування ігор за різними параметрами.

🛒 Кошик – Додавання товарів, редагування кількості, видалення.

🛍️ Оформлення замовлення – Вибір способу оплати, введення контактних даних.

🎟️ Знижки – Впроваджено систему динамічного ціноутворення.

🔑 Авторизація та ролі – Реалізовано розмежування доступу для користувачів і адміністраторів.

📊 Адмін-панель – Управління товарами, замовленнями та користувачами.

📧 Email-сповіщення – Підтвердження замовлень через email.

🏗️ Патерни проектування – Використано Фасад, Стратегію, Одинак для покращення архітектури.

🛠️ Використані технології

Backend: ASP.NET Core, Entity Framework Core, FluentValidation

Frontend: Bootstrap 5, JavaScript (jQuery, Swiper.js, Choices.js)

Database: Microsoft SQL Server

Logging: Serilog

Dependency Injection: Впроваджено через IServiceCollection

Docker: Налаштовано для розгортання з MSSQL

⚙️ Запуск проекту

1. Клонування репозиторію

git clone https://github.com/yourusername/GameStore.git
cd GameStore

2. Налаштування середовища

2.1. - Створення .env файлу (або змінних середовища)

DB_CONNECTION_STRING=Server=localhost;Database=GameStoreDB;User Id=sa;Password=YourPassword;

2.2. - Налаштування appsettings.json

Вкажіть правильний рядок підключення до бази даних та параметри SMTP для email-сповіщень.

3. Запуск через Docker

docker-compose up --build

Примітка: У Docker-файлі налаштований SQL Server + ASP.NET Core API.

4. Локальний запуск без Docker

4.1. - Міграція бази даних

dotnet ef database update

4.2. - Запуск застосунку

dotnet run

📌 Дорожня карта

✅ Реалізація каталогу товарів
✅ Авторизація та ролі (Admin/User)
✅ Обробка замовлень та оплати
✅ Динамічні знижки
🔄 Оптимізація запитів до бази
🔄 Інтеграція з платіжними системами

⭐ Якщо тобі сподобався проєкт – постав зірочку на GitHub! ⭐

