using GoBetGoal_BackEnd.Filters;
using GoBetGoal_BackEnd.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Web.Http;

namespace GoBetGoal_BackEnd
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // --- 2. CORS 設定 ---
            // 讓您的前端可以順利地呼叫後端 API
            //// 在正式上線時，建議將 origins 改為您前端的實際網址，例如 "http://your-frontend-domain.com"

            //var cors = new EnableCorsAttribute(
            //    origins: "*", // 在正式環境建議換成您的前端網址
            //    headers: "*",
            //    methods: "*"
            //);
            //config.EnableCors(cors);

            // --- 3. 全局 Filter 設定 ---
            // 注意順序：通常 ExceptionFilter 在最前，AuthFilter 在後
            // 將我們寫好的 GlobalExceptionFilter 註冊為全局過濾器
            config.Filters.Add(new GlobalExceptionFilter());
            // *** 將您的 JwtAuthFilter 註冊為全局過濾器 ***
            // 從現在開始，每一支 API 預設都需要通過 JWT 驗證
            //config.Filters.Add(new JwtAuthFilter());
            config.Filters.Add(new ValidateModelAttribute()); // 註冊模型驗證篩選器
            config.Filters.Add(new JwtAuthFilter());

            // --- 4. JSON Formatter 設定 ---
            // 取得 JSON Formatter 的設定物件
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;

            // a. 將屬性名稱自動轉換為 camelCase (小駝峰式)
            jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // b. 將 Enum 轉換為字串，而不是數字
            jsonSettings.Converters.Add(new StringEnumConverter());

            // c. 忽略值為 null 的屬性，讓回傳的 JSON 更乾淨
            //jsonSettings.NullValueHandling = NullValueHandling.Ignore;

            // d. 避免因 Entity Framework 導覽屬性造成的循環參考錯誤
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // --- 5. 移除 XML Formatter ---
            // 讓 API 只支援回傳 JSON 格式，更單純、更安全
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
