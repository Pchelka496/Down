English

Game Description
A game where the player controls a satellite descending from orbit to the surface. During this journey, the player must dodge obstacles in the form of enemies, collect coins for upgrades, and pick up repair kits to restore health.

Technical Highlights:

Asynchronous Programming: The project heavily utilizes asynchronous code. For enemy movement, the Job System was used, making all their behavior (including rotation) multithreaded for maximum efficiency.

Addressables: Since the game consists of a single scene, I use Addressables to dynamically load and unload necessary or unnecessary assets.

Global Events Manager: A dedicated GlobalEventsManager class handles transitions between game modes (e.g., lobby mode and game mode).

Modular Player System:

The satellite is built with a modular system where each module has a specific responsibility.

CoreModules: Mandatory modules essential for gameplay, such as EngineModule, HealthModule, PickerModule, and RotationModule. These modules cannot be disabled, as they are critical for the core mechanics, but the game will still function if removed (albeit with limited gameplay).

OptionalModules: Additional modules that can be purchased and toggled on or off at will, offering customization and new abilities.

Game Features:
*ScriptableObject-Based Configuration: All game mechanics and behaviors are configured using ScriptableObject for scalability and ease of use.
*Save System: Implemented using JSON for efficient and straightforward data management.
*Ads and Analytics: Integrated with AdMob for monetization and Unity Analytics for tracking player behavior.
*Customization: Players can customize their satellite with skins and modify the controller’s appearance by applying a personalized gradient.
*Update-Free Logic: The game avoids using coroutines and traditional Update loops wherever possible. Instead, asynchronous tasks handle most game logic, with only a single Update used for specific scenarios.

Russian

Описание игры
Игра, где игрок управляет спутником, который должен опуститься с орбиты на поверхность, уклоняясь от препятствий в виде врагов, собирая монетки для прокачки и ремонтные наборы для восстановления здоровья.

Технические особенности:

Асинхронное программирование: В проекте используется большое количество асинхронного кода. Для перемещения врагов применён Job System, благодаря чему все их поведение (включая вращение) реализовано многопоточно для максимальной эффективности.

Addressables: Поскольку в игре всего одна сцена, загрузка и выгрузка ассетов осуществляется через Addressables.

Глобальный менеджер событий: Класс GlobalEventsManager управляет переходами между режимами игры (например, режим лобби и игровой режим).

Модульная система игрока:
Спутник построен с использованием модульной системы, где каждый модуль имеет свою зону ответственности.

CoreModules: Основные модули, обязательные для работы ключевых механик игры (EngineModule, HealthModule, PickerModule, RotationModule и другие). Эти модули нельзя отключить, так как они критически важны, но при их удалении игра всё равно продолжит работать с ограниченным функционалом.

OptionalModules: Необязательные модули, которые можно докупать отдельно и включать/выключать по желанию.

Особенности игры:
*Конфигурация через ScriptableObject: Вся игровая логика и механики настраиваются через ScriptableObject для удобства и масштабируемости.
*Система сохранений: Реализована через JSON, обеспечивая простоту и эффективность.
*Реклама и аналитика: Интеграция с AdMob для монетизации и Unity Analytics для отслеживания поведения игроков.
*Кастомизация: Игроки могут кастомизировать свой спутник скинами и изменять внешний вид контроллера, применяя свой собственный градиент.
*Логика без Update: В проекте избегается использование корутин и традиционного Update. Большинство логики реализовано через асинхронные задачи, а Update используется только в одном случае.
