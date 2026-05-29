using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient();
        var content = new StringContent("{\"contents\":[{\"parts\":[{\"text\":\"hello\"}]}]}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=AIzaSyDpdCxusbEBRoVCT4TY1TCzwZynpTGRxuI", content);
        Console.WriteLine((int)response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }
}
