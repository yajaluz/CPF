using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPF.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private static IConfiguration Configuration;
        IList<string> cpfs = new List<string>();

        static string diretorio = "", id = "";
        string pathDirectory = "", fileName = "";

        public ValuesController()
        {
            VerifyFileDirectory();
        }


        #region REST
        [HttpGet]
        public List<string> Get()
        {
            return (List<string>)cpfs;
        }


        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return cpfs.Where(x => x == id).ToString();
        }

        // POST api/values
        [HttpPost("{id}")]
        public void Post(string id)
        {
            if (VerifyMaskAndDigitCount(id) && VerifyLastDigit(ValuesController.id) && VerifyPenultimateDigit(ValuesController.id))
            {
                try
                {
                    var stream = new FileStream(fileName, FileMode.Open, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(stream);

                    sw.WriteLine(id);

                    sw.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
            }

        }

        // PUT api/values/5
        [HttpPut("{id}/{value}")]
        public void Put(string id, string value)
        {
            if (VerifyMaskAndDigitCount(id) && VerifyLastDigit(ValuesController.id) && VerifyPenultimateDigit(ValuesController.id))
            {
                try
                {
                    var texto = System.IO.File.ReadAllText(fileName).Replace(id, value);
                    System.IO.File.WriteAllText(fileName, texto);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            if (VerifyMaskAndDigitCount(id) && VerifyLastDigit(ValuesController.id) && VerifyPenultimateDigit(ValuesController.id))
            {
                try
                {
                    var texto = System.IO.File.ReadAllText(fileName).Replace(id, "");
                    System.IO.File.WriteAllText(fileName, texto);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
            }
        }

        // VERIFY CPF api/values/5
        [HttpGet("Verify/{id}")]
        public IActionResult VerifyCPF(string id)
        {
            if (VerifyMaskAndDigitCount(id) && VerifyLastDigit(ValuesController.id) && VerifyPenultimateDigit(ValuesController.id))
                return Ok();

            return BadRequest();
        }
        #endregion

        #region Verify
        private static string GetParamString(string param)
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            var connectionString = Configuration["Names:" + param];


            return connectionString;
        }

        private void VerifyFileDirectory()
        {
            diretorio = String.Join(" ", Directory.GetCurrentDirectory());
            pathDirectory = String.Concat(diretorio, GetParamString("DirectoryName"));

            if (!Directory.Exists(pathDirectory))
                Directory.CreateDirectory(pathDirectory);

            fileName = String.Concat(pathDirectory, GetParamString("FileName"));

            if (!System.IO.File.Exists(fileName))
                System.IO.File.Create(fileName.ToString());
            else
                cpfs = System.IO.File.ReadAllLines(fileName.ToString());

        }

        private static bool VerifyMaskAndDigitCount(string id)
        {
            var count = id.ToCharArray();

            if (count.Length == 11)
            {
                if (count.Any(c => !c.Equals(count.First())))
                    if (count.All(x => char.IsNumber(Convert.ToChar(x))))
                    {
                        ValuesController.id = id;
                        return true;
                    }
            }
            else if (count.Length > 11)
            {
             
                while (id.Contains('-') ||  id.Contains('.') || id.Contains('/'))
                {

                    if (id.Contains('-'))
                        id = id.Remove(id.IndexOf('-'), 1);


                    if (id.Contains('/'))
                        id = id.Remove(id.IndexOf('/'), 1);

                    if (id.Contains('.'))
                        id = id.Remove(id.IndexOf('.'), 1);

                }

                ValuesController.id = id;
                count = id.ToCharArray();

                if (count.All(x => char.IsNumber(Convert.ToChar(x))))
                    if(ValuesController.id.Length == 11)
                        return true;
            }

            return false;

        }

        private static bool VerifyLastDigit(string id)
        {
            int sumLastDigit = 0;
            var digits = id.ToCharArray();
            var last = Char.GetNumericValue(digits.Last());

            for (int x = 0, y = 11; x <= digits.Length - 2; x++, y--)
                sumLastDigit += Convert.ToInt32(Char.GetNumericValue(digits[x])) * y;


            var restLast = sumLastDigit % 11;

            if (restLast == 0 || restLast == 1)
            {
                if (last == restLast)
                    return true;
            }
            else if (restLast > 1)
                if (11 - restLast == last)
                    return true;

            return false;
        }

        private static bool VerifyPenultimateDigit(string id)
        {
            int sumPenultimateDigit = 0;
            var digits = id.ToCharArray();
            var penultimate = Char.GetNumericValue(digits[^2]);


            for (int x = 0, y = 10; x <= digits.Length - 3; x++, y--)
                sumPenultimateDigit += Convert.ToInt32(Char.GetNumericValue(digits[x])) * y;

            var restPenultimate = sumPenultimateDigit % 11;

            if (restPenultimate == 0 || restPenultimate == 1)
            {
                if (penultimate == restPenultimate)
                    return true;
            }
            else if (restPenultimate > 1)
                if (11 - restPenultimate == penultimate)
                    return true;

            return false;

        }
    }
    #endregion
}
