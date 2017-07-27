using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonContracts;

namespace WebAPIClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Process();
            Console.Read();
        }

        private async static void Process()
        {
            string baseURL = "http://172.28.5.54:10001";
            HttpClient httpClient = new HttpClient();
            string apiURL = "api/Books";
            HttpResponseMessage response = await httpClient.GetAsync(string.Format("{0}/{1}", baseURL, apiURL));
            string responseValue = await response.Content.ReadAsStringAsync();
            IEnumerable<Book> books = await response.Content.ReadAsAsync<IEnumerable<Book>>();
            foreach (Book book in books)
            {
                Console.WriteLine(book.Id + "  " + book.Title);
            }
        }
    }
}
