using NutritionApp.Models;

namespace NutritionApp.Data
{
    public static class ProductsData
    {
        public static List<ProductItem> GetAllProducts()
        {
            return new List<ProductItem>
            {
                // === М'ЯСО ТА ПТИЦЯ ===
                new ProductItem("Куряча грудка", "М'ясо", "🍗"),
                new ProductItem("Куряче стегно", "М'ясо", "🍗"),
                new ProductItem("Курячий фарш", "М'ясо", "🍗"),
                new ProductItem("Індичка (філе)", "М'ясо", "🦃"),
                new ProductItem("Яловичина", "М'ясо", "🥩"),
                new ProductItem("Свинина", "М'ясо", "🥓"),
                new ProductItem("Свинячий фарш", "М'ясо", "🥓"),
                new ProductItem("Ковбаса варена", "М'ясо", "🌭"),
                new ProductItem("Сосиски", "М'ясо", "🌭"),
                new ProductItem("Шинка", "М'ясо", "🥓"),
                new ProductItem("Бекон", "М'ясо", "🥓"),
                new ProductItem("Печінка курячa", "М'ясо", "🍖"),
                new ProductItem("Печінка яловича", "М'ясо", "🍖"),

                // === РИБА ТА МОРЕПРОДУКТИ ===
                new ProductItem("Лосось", "Риба", "🐟"),
                new ProductItem("Скумбрія", "Риба", "🐟"),
                new ProductItem("Тунець (консерва)", "Риба", "🐟"),
                new ProductItem("Мінтай", "Риба", "🐟"),
                new ProductItem("Хек", "Риба", "🐟"),
                new ProductItem("Тріска", "Риба", "🐟"),
                new ProductItem("Оселедець", "Риба", "🐟"),
                new ProductItem("Креветки", "Риба", "🦐"),
                new ProductItem("Кальмар", "Риба", "🦑"),
                new ProductItem("Рибні палички", "Риба", "🐟"),

                // === МОЛОЧНІ ПРОДУКТИ ===
                new ProductItem("Молоко", "Молочка", "🥛"),
                new ProductItem("Кефір", "Молочка", "🥛"),
                new ProductItem("Йогурт натуральний", "Молочка", "🥛"),
                new ProductItem("Сир кисломолочний", "Молочка", "🧀"),
                new ProductItem("Сир твердий", "Молочка", "🧀"),
                new ProductItem("Сир плавлений", "Молочка", "🧀"),
                new ProductItem("Бринза", "Молочка", "🧀"),
                new ProductItem("Сметана", "Молочка", "🥛"),
                new ProductItem("Вершки", "Молочка", "🥛"),
                new ProductItem("Масло вершкове", "Молочка", "🧈"),
                new ProductItem("Ряжанка", "Молочка", "🥛"),
                new ProductItem("Сирок глазурований", "Молочка", "🍫"),

                // === ЯЙЦЯ ===
                new ProductItem("Яйця курячі", "Яйця", "🥚"),
                new ProductItem("Яйця перепелині", "Яйця", "🥚"),

                // === КРУПИ ТА МАКАРОНИ ===
                new ProductItem("Рис білий", "Крупи", "🍚"),
                new ProductItem("Рис бурий", "Крупи", "🍚"),
                new ProductItem("Гречка", "Крупи", "🌾"),
                new ProductItem("Вівсянка", "Крупи", "🥣"),
                new ProductItem("Пшоно", "Крупи", "🌾"),
                new ProductItem("Перловка", "Крупи", "🌾"),
                new ProductItem("Кускус", "Крупи", "🌾"),
                new ProductItem("Булгур", "Крупи", "🌾"),
                new ProductItem("Манка", "Крупи", "🌾"),
                new ProductItem("Макарони", "Крупи", "🍝"),
                new ProductItem("Спагеті", "Крупи", "🍝"),
                new ProductItem("Локшина", "Крупи", "🍜"),
                new ProductItem("Кукурудзяна крупа", "Крупи", "🌽"),

                // === ХЛІБ ТА ВИПІЧКА ===
                new ProductItem("Хліб білий", "Хліб", "🍞"),
                new ProductItem("Хліб чорний", "Хліб", "🍞"),
                new ProductItem("Хліб цільнозерновий", "Хліб", "🍞"),
                new ProductItem("Батон", "Хліб", "🥖"),
                new ProductItem("Лаваш", "Хліб", "🫓"),
                new ProductItem("Хлібці", "Хліб", "🍞"),
                new ProductItem("Булочка", "Хліб", "🥐"),
                new ProductItem("Тост", "Хліб", "🍞"),

                // === ОВОЧІ ===
                new ProductItem("Картопля", "Овочі", "🥔"),
                new ProductItem("Морква", "Овочі", "🥕"),
                new ProductItem("Цибуля", "Овочі", "🧅"),
                new ProductItem("Часник", "Овочі", "🧄"),
                new ProductItem("Буряк", "Овочі", "🥬"),
                new ProductItem("Капуста білокачанна", "Овочі", "🥬"),
                new ProductItem("Капуста цвітна", "Овочі", "🥦"),
                new ProductItem("Броколі", "Овочі", "🥦"),
                new ProductItem("Помідори", "Овочі", "🍅"),
                new ProductItem("Огірки", "Овочі", "🥒"),
                new ProductItem("Перець болгарський", "Овочі", "🫑"),
                new ProductItem("Кабачок", "Овочі", "🥒"),
                new ProductItem("Баклажан", "Овочі", "🍆"),
                new ProductItem("Гарбуз", "Овочі", "🎃"),
                new ProductItem("Шпинат", "Овочі", "🥬"),
                new ProductItem("Салат листовий", "Овочі", "🥬"),
                new ProductItem("Редиска", "Овочі", "🥬"),
                new ProductItem("Горошок зелений", "Овочі", "🫛"),
                new ProductItem("Кукурудза (консерва)", "Овочі", "🌽"),
                new ProductItem("Квасоля (консерва)", "Овочі", "🫘"),
                new ProductItem("Гриби печериці", "Овочі", "🍄"),
                new ProductItem("Гриби шампіньйони", "Овочі", "🍄"),
                new ProductItem("Зелень (укроп, петрушка)", "Овочі", "🌿"),

                // === ФРУКТИ ТА ЯГОДИ ===
                new ProductItem("Яблуко", "Фрукти", "🍎"),
                new ProductItem("Банан", "Фрукти", "🍌"),
                new ProductItem("Апельсин", "Фрукти", "🍊"),
                new ProductItem("Мандарин", "Фрукти", "🍊"),
                new ProductItem("Лимон", "Фрукти", "🍋"),
                new ProductItem("Груша", "Фрукти", "🍐"),
                new ProductItem("Виноград", "Фрукти", "🍇"),
                new ProductItem("Ківі", "Фрукти", "🥝"),
                new ProductItem("Полуниця", "Фрукти", "🍓"),
                new ProductItem("Малина", "Фрукти", "🫐"),
                new ProductItem("Чорниця", "Фрукти", "🫐"),
                new ProductItem("Персик", "Фрукти", "🍑"),
                new ProductItem("Слива", "Фрукти", "🫐"),
                new ProductItem("Кавун", "Фрукти", "🍉"),
                new ProductItem("Диня", "Фрукти", "🍈"),
                new ProductItem("Авокадо", "Фрукти", "🥑"),
                new ProductItem("Сухофрукти (курага)", "Фрукти", "🍇"),
                new ProductItem("Сухофрукти (родзинки)", "Фрукти", "🍇"),
                new ProductItem("Сухофрукти (чорнослив)", "Фрукти", "🍇"),

                // === ГОРІХИ ТА НАСІННЯ ===
                new ProductItem("Волоські горіхи", "Горіхи", "🥜"),
                new ProductItem("Мигдаль", "Горіхи", "🥜"),
                new ProductItem("Кешью", "Горіхи", "🥜"),
                new ProductItem("Фундук", "Горіхи", "🥜"),
                new ProductItem("Арахіс", "Горіхи", "🥜"),
                new ProductItem("Насіння соняшника", "Горіхи", "🌻"),
                new ProductItem("Насіння гарбуза", "Горіхи", "🎃"),
                new ProductItem("Насіння льону", "Горіхи", "🌾"),
                new ProductItem("Насіння чіа", "Горіхи", "🌾"),
                new ProductItem("Кунжут", "Горіхи", "🌾"),

                // === ОЛІЇ ТА СОУСИ ===
                new ProductItem("Олія соняшникова", "Олії", "🫒"),
                new ProductItem("Олія оливкова", "Олії", "🫒"),
                new ProductItem("Олія кокосова", "Олії", "🥥"),
                new ProductItem("Майонез", "Олії", "🥣"),
                new ProductItem("Кетчуп", "Олії", "🍅"),
                new ProductItem("Гірчиця", "Олії", "🌭"),
                new ProductItem("Соєвий соус", "Олії", "🥢"),
                new ProductItem("Оцет", "Олії", "🍶"),

                // === БОБОВІ ===
                new ProductItem("Сочевиця", "Бобові", "🫘"),
                new ProductItem("Нут", "Бобові", "🫘"),
                new ProductItem("Квасоля біла", "Бобові", "🫘"),
                new ProductItem("Квасоля червона", "Бобові", "🫘"),
                new ProductItem("Горох", "Бобові", "🫛"),

                // === СОЛОДОЩІ ТА СНЕКИ ===
                new ProductItem("Мед", "Солодощі", "🍯"),
                new ProductItem("Цукор", "Солодощі", "🧂"),
                new ProductItem("Шоколад чорний", "Солодощі", "🍫"),
                new ProductItem("Варення", "Солодощі", "🍓"),
                new ProductItem("Печиво", "Солодощі", "🍪"),
                new ProductItem("Мюслі/гранола", "Солодощі", "🥣"),

                // === НАПОЇ ТА ІНШЕ ===
                new ProductItem("Чай", "Напої", "🍵"),
                new ProductItem("Кава", "Напої", "☕"),
                new ProductItem("Какао", "Напої", "🍫"),
                new ProductItem("Сік апельсиновий", "Напої", "🧃"),
                new ProductItem("Сік яблучний", "Напої", "🧃"),
                new ProductItem("Вода мінеральна", "Напої", "💧"),

                // === ПРИПРАВИ ===
                new ProductItem("Сіль", "Приправи", "🧂"),
                new ProductItem("Перець чорний", "Приправи", "🌶️"),
                new ProductItem("Паприка", "Приправи", "🌶️"),
                new ProductItem("Куркума", "Приправи", "🌿"),
                new ProductItem("Орегано", "Приправи", "🌿"),
                new ProductItem("Базилік", "Приправи", "🌿"),
                new ProductItem("Лавровий лист", "Приправи", "🍃"),

                // === ЗАМОРОЖЕНІ ПРОДУКТИ ===
                new ProductItem("Заморожені овочі (суміш)", "Заморожене", "❄️"),
                new ProductItem("Заморожені ягоди", "Заморожене", "❄️"),
                new ProductItem("Заморожена риба", "Заморожене", "❄️"),
                new ProductItem("Пельмені", "Заморожене", "🥟"),
                new ProductItem("Вареники", "Заморожене", "🥟"),
            };
        }

        public static List<string> GetCategories()
        {
            return new List<string>
            {
                "М'ясо",
                "Риба",
                "Молочка",
                "Яйця",
                "Крупи",
                "Хліб",
                "Овочі",
                "Фрукти",
                "Горіхи",
                "Олії",
                "Бобові",
                "Солодощі",
                "Напої",
                "Приправи",
                "Заморожене"
            };
        }
    }
}
