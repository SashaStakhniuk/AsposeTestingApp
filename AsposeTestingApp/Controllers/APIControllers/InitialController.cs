using AsposeTestingApp.Models.ViewModels;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AsposeTestingApp.Controllers.APIControllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class InitialController : Controller
    {

        private readonly List<string> excludedWords = null; // слова,які не враховуються в загальний список
        readonly char[] separator; // символи розділення тексту на масив слів

        public InitialController()
        {
            excludedWords = new List<string> { "a", "an", "the", "on", "is", "to" };//,  "off", "up"};
            //separator = new string[] { " ", ",", ". ", "\n", "\t" };
            separator = new char[] { ' ', ',', '.', ':', ';', '!', '&', '?', '\0', '\n', '\t', '\r', '\u00A0', '\u0001', '\u2014', '\u0022', '=', '`', '{', '}', '(', ')', '-', '+', '^', '@', '#', '$', '%', '*', '/', '|', '\\', '[', ']', '\'', '"' };
            //var separator = Enumerable.Range(0, char.MaxValue + 1)
            //          .Select(i => (char)i)
            //          .Where(c => char.IsSymbol(c))
            //          .ToArray();
        }

        //private async Task<List<string>> SplitTextAsync(int wordsToSplit = 2, string text = "")
        private List<string> SplitTextAsync(int wordsToSplit = 2, string text = "")
        {
            //List<string> words = model.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList(); //розділяю текст на масив слів. Працює тільки для розділення по 1 слову
            // можна переробити лямбду для розбиття на 2 слова. І так кожного разу її треба буде переробляти.
            List<string> words = new List<string>(); // створюю список розділених слів/фраз

            string word = ""; // слово із тексту
            string phraseToAdd = "";// слово/фраза для додавання у список
            ushort amountOfWords = 0;// тригер розділення на к-сть слів


            for (int i = 0; i < text.Length; i++) // перебираю усі символи тексту
            {

                if (separator.Any(item => item == text[i])) // якщо розділовий знак
                {
                    if (!String.IsNullOrWhiteSpace(word)) // якщо слово не порожнє
                    {
                        phraseToAdd += " " + word; // додаю слово у фразу
                        //amountOfWords++; // додаю лічильник на 1 слово
                        if (++amountOfWords < wordsToSplit) // якщо потрібно аналізувати > 1 слова
                        {
                            word = ""; // зануляю нове слово
                                       //new Thread(() =>
                                       //{ }).Start();

                            for (int j = i; j < text.Length; j++) // починаючи від останнього символа (розділовий знак) і до кінця тексту 
                            {
                                if (j == text.Length - 1)// якщо дійшли до кінця списку,
                                {
                                    phraseToAdd += " " + word; // додаємо слово до фрази
                                    words.Add(phraseToAdd.Trim()); // додаю фразу до списку
                                    return words; // повертаю список слів
                                }
                                if (separator.Any(item => item == text[j])) // якщо розділовий знак
                                {
                                    //if (text[j] == '.' || text[j] == '!' || text[j] == '?') // не з'єдную слова у фразу, що розділені . ! ?
                                    //{
                                    //    i = j + 1;
                                    //}

                                    if (!String.IsNullOrWhiteSpace(word)) // якщо слово не порожнє
                                    {
                                        if (!excludedWords.Any(x => x == word)) // якщо слово = винятку
                                        {
                                            phraseToAdd += " " + word; // додаємо слово до фрази
                                            word = "";
                                            //amountOfWords++; // додаю лічильник
                                            if (++amountOfWords == wordsToSplit)
                                            {
                                                words.Add(phraseToAdd.Trim()); // додаю фразу до списку
                                                amountOfWords = 0;// занулюю лічильник
                                                phraseToAdd = "";//занулюю фразу
                                                break; // коли слова знайдено та додано, зупиняю перебір і повертаюсь до наступного слова 
                                            }
                                        }
                                        else
                                        {
                                            word = "";
                                        }

                                    }
                                }
                                else
                                {
                                    word += text[j]; // додаю символ до слова
                                }
                            }
                        }
                        else if (amountOfWords == wordsToSplit) // якщо к-сть слів = потрібній
                        {
                            words.Add(phraseToAdd.Trim()); // додаю слово/фразу у список
                            phraseToAdd = "";
                            amountOfWords = 0; // занулюю лічильник
                            word = "";
                        }
                    }
                }
                else
                {
                    word += text[i]; // додаю символ до слова
                }
            }

            return words;
        }

        [HttpPost]
        public async Task<ActionResult<ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>>> AnalyzeTextAsync(AnalyzeViewModel model)
        //public ActionResult<ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>> AnalyzeText(AnalyzeViewModel model)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(model.Text))
                {
                    return StatusCode(400, "Text not found");
                }

                //if (model.Top > 0)
                //{
                //    Top = model.Top;
                //}
                //else
                //{
                //    return StatusCode(400, "Top can't be <= 0");
                //}
                if (model.WordsToSplit <= 0)
                {
                    model.WordsToSplit = 1;
                }
                await Task.Run(async () => // перевіряю чи текст це посилання
                {
                    bool textIsLink = Uri.TryCreate(model.Text, UriKind.Absolute, out Uri uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                    if (textIsLink) // якщо так, тоді завантажую html контент із сторінки
                    {
                        using var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("User-Agent", "C# ASPOSE testing program");

                        var htmlString = await client.GetStringAsync(model.Text);

                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(htmlString);

                        model.Text = doc.DocumentNode.SelectSingleNode("//body").InnerText; // дістаю ввесь текст
                    }
                });


                List<string> words = this.SplitTextAsync(model.WordsToSplit, model.Text + ' ');// додаю розділовий знак в кінець тексту 

                //words.ForEach(x => Console.WriteLine(x));

                int totalWordsAmount = words.Count; // загальна к-сть слів

                List<string> removedWords = new List<string>(); // список видалених артиклів

                //var deletedWords = words.RemoveAll(x => excludedWords.Any(item => item == x)); // видаляю усі слова-артиклі і т.п

               
                    words.RemoveAll(x =>
                    {
                        //var wordFound = excludedWords.Any(item => x.Contains(item));

                        var wordFound = excludedWords.Any(item => item == x);
                        if (wordFound)
                        {
                            removedWords.Add(x);
                        }
                        return wordFound;
                    }); // видаляю усі артиклі і т.п. Записую видалені в новий список

                removedWords = removedWords.Distinct().ToList(); // видаляю повтори, залишаю список унікальних видалених елементів (винятки із excludedWords )

                //words.ForEach(x => Console.WriteLine(x));

                var statisticList = new List<ValueTuple<string, int, float>>();//список кортежів (назва,к-сть входжень, відсотки)

                bool wordAlreadyExist; // перевірка на те, чи слово вже існує в списку


                for (int i = 0; i < words.Count; i++) // перебираю кожне слово даного тексту
                {
                    await Task.Run(() =>
                    {
                        var tupleToAdd = statisticList.FirstOrDefault(x => x.Item1 == words[i].ToLower());// шукаю чи кортеж з обраним словом вже існує

                        if (String.IsNullOrWhiteSpace(tupleToAdd.Item1)) // якщо не існує
                        {
                            wordAlreadyExist = false;
                            tupleToAdd = new ValueTuple<string, int, float>(words[i].ToLower(), 1, 0); // створюю новий кортеж, додаю в нього обране слово
                        }
                        else
                        {
                            wordAlreadyExist = true;
                        }

                        for (int j = i + 1; j < (words.Count - 1); j++) // починаючи від наступного від обраного слова і до кінця списку
                        {
                            if (tupleToAdd.Item1 == words[j].ToLower()) // якщо слово повторюється
                            {
                                tupleToAdd.Item2 += 1; // додаю його до загальної к-сті
                            }
                        }

                        //tupleToAdd.Item3 = (float)tupleToAdd.Item2 * 100 / totalWordsAmount; // вираховую відсоткове значення слова від загальної к-сті слів
                        tupleToAdd.Item3 = (float)Math.Round((decimal)tupleToAdd.Item2 * 100 / totalWordsAmount, 3); // вираховую відсоткове значення слова від загальної к-сті слів
                        if (!wordAlreadyExist) // якщо обраного слова ще не було у списку
                        {
                            statisticList.Add(tupleToAdd); // додаю до списку значень
                        }
                    });
                }
                //statisticList = statisticList.OrderByDescending(x => x.Item3).ThenBy(x => x.Item1).ToList();
                statisticList = statisticList.OrderByDescending(x => x.Item3).ToList();

                var uniqueWordsAmount = statisticList.Count + removedWords.Count;
                return Ok(new ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>(totalWordsAmount, uniqueWordsAmount, statisticList)); // загальна к-сть слів, к-сть унікальних слів, інша ін-фа
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}






//using AsposeTestingApp.Models.ViewModels;
//using HtmlAgilityPack;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Xml;

//namespace AsposeTestingApp.Controllers.APIControllers
//{
//    [ApiController]
//    [Route("api/[controller]/[action]")]
//    public class InitialController : Controller
//    {

//        private readonly List<string> excludedWords = null; // слова,які не враховуються в загальний список
//        readonly char[] separator; // символи розділення тексту на масив слів

//        public InitialController()
//        {
//            excludedWords = new List<string> { "a", "an", "the", "on", "is", "to" };//,  "off", "up"};
//            //separator = new string[] { " ", ",", ". ", "\n", "\t" };
//            separator = new char[] { ' ', ',', '.', ':', ';', '!', '&', '?', '\0', '\n', '\t', '\r', '\u00A0', '\u0001', '\u2014', '\u0022', '=', '`', '{', '}', '(', ')', '-', '+', '^', '@', '#', '$', '%', '*', '/', '|', '\\', '[', ']', '\'', '"' };
//            //var separator = Enumerable.Range(0, char.MaxValue + 1)
//            //          .Select(i => (char)i)
//            //          .Where(c => char.IsSymbol(c))
//            //          .ToArray();
//        }

//        //private async Task<List<string>> SplitTextAsync(int wordsToSplit = 2, string text = "")
//        private List<string> SplitTextAsync(int wordsToSplit = 2, string text = "")
//        {
//            //List<string> words = model.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries).ToList(); //розділяю текст на масив слів. Працює тільки для розділення по 1 слову
//            // можна переробити лямбду для розбиття на 2 слова. І так кожного разу її треба буде переробляти.
//            List<string> words = new List<string>(); // створюю список розділених слів/фраз

//            string word = ""; // слово із тексту
//            string phraseToAdd = "";// слово/фраза для додавання у список
//            ushort amountOfWords = 0;// тригер розділення на к-сть слів


//            for (int i = 0; i < text.Length; i++) // перебираю усі символи тексту
//            {

//                if (separator.Any(item => item == text[i])) // якщо розділовий знак
//                {
//                    if (!String.IsNullOrWhiteSpace(word)) // якщо слово не порожнє
//                    {
//                        phraseToAdd += " " + word; // додаю слово у фразу
//                        //amountOfWords++; // додаю лічильник на 1 слово
//                        if (++amountOfWords < wordsToSplit) // якщо потрібно аналізувати > 1 слова
//                        {
//                            word = ""; // зануляю нове слово
//                                       //new Thread(() =>
//                                       //{ }).Start();

//                            for (int j = i; j < text.Length; j++) // починаючи від останнього символа (розділовий знак) і до кінця тексту 
//                            {
//                                if (j == text.Length - 1)// якщо дійшли до кінця списку,
//                                {
//                                    phraseToAdd += " " + word; // додаємо слово до фрази
//                                    words.Add(phraseToAdd.Trim()); // додаю фразу до списку
//                                    return words; // повертаю список слів
//                                }
//                                if (separator.Any(item => item == text[j])) // якщо розділовий знак
//                                {
//                                    //if (text[j] == '.' || text[j] == '!' || text[j] == '?') // не з'єдную слова у фразу, що розділені . ! ?
//                                    //{
//                                    //    i = j + 1;
//                                    //}

//                                    if (!String.IsNullOrWhiteSpace(word)) // якщо слово не порожнє
//                                    {
//                                        if (!excludedWords.Any(x => x == word)) // якщо слово = винятку
//                                        {
//                                            phraseToAdd += " " + word; // додаємо слово до фрази
//                                            word = "";
//                                            //amountOfWords++; // додаю лічильник
//                                            if (++amountOfWords == wordsToSplit)
//                                            {
//                                                words.Add(phraseToAdd.Trim()); // додаю фразу до списку
//                                                amountOfWords = 0;// занулюю лічильник
//                                                phraseToAdd = "";//занулюю фразу
//                                                break; // коли слова знайдено та додано, зупиняю перебір і повертаюсь до наступного слова 
//                                            }
//                                        }
//                                        else
//                                        {
//                                            word = "";
//                                        }

//                                    }
//                                }
//                                else
//                                {
//                                    word += text[j]; // додаю символ до слова
//                                }
//                            }
//                        }
//                        else if (amountOfWords == wordsToSplit) // якщо к-сть слів = потрібній
//                        {
//                            words.Add(phraseToAdd.Trim()); // додаю слово/фразу у список
//                            phraseToAdd = "";
//                            amountOfWords = 0; // занулюю лічильник
//                            word = "";
//                        }
//                    }
//                }
//                else
//                {
//                    word += text[i]; // додаю символ до слова
//                }
//            }

//            return words;
//        }

//        [HttpPost]
//        public async Task<ActionResult<ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>>> AnalyzeTextAsync(AnalyzeViewModel model)
//        //public ActionResult<ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>> AnalyzeText(AnalyzeViewModel model)
//        {
//            try
//            {
//                if (String.IsNullOrWhiteSpace(model.Text))
//                {
//                    return StatusCode(400, "Text not found");
//                }

//                //if (model.Top > 0)
//                //{
//                //    Top = model.Top;
//                //}
//                //else
//                //{
//                //    return StatusCode(400, "Top can't be <= 0");
//                //}
//                if (model.WordsToSplit <= 0)
//                {
//                    model.WordsToSplit = 1;
//                }
//                await Task.Run(async () => // перевіряю чи текст це посилання
//                {
//                    bool textIsLink = Uri.TryCreate(model.Text, UriKind.Absolute, out Uri uriResult)
//                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
//                    if (textIsLink) // якщо так, тоді завантажую html контент із сторінки
//                    {
//                        using var client = new HttpClient();
//                        client.DefaultRequestHeaders.Add("User-Agent", "C# ASPOSE testing program");

//                        var htmlString = await client.GetStringAsync(model.Text);

//                        HtmlDocument doc = new HtmlDocument();
//                        doc.LoadHtml(htmlString);

//                        model.Text = doc.DocumentNode.SelectSingleNode("//body").InnerText; // дістаю ввесь текст
//                    }
//                });


//                List<string> words = this.SplitTextAsync(model.WordsToSplit, model.Text + ' ');// додаю розділовий знак в кінець тексту

//                //words.ForEach(x => Console.WriteLine(x));

//                int totalWordsAmount = words.Count; // загальна к-сть слів

//                List<string> removedWords = new List<string>(); // список видалених артиклів

//                //var deletedWords = words.RemoveAll(x => excludedWords.Any(item => item == x)); // видаляю усі слова-артиклі і т.п
//                await Task.Run(() =>
//                {
//                    words.RemoveAll(x =>
//                    {
//                        //var wordFound = excludedWords.Any(item => x.Contains(item));

//                        var wordFound = excludedWords.Any(item => item == x);
//                        if (wordFound)
//                        {
//                            removedWords.Add(x);
//                        }
//                        return wordFound;
//                    }); // видаляю усі артиклі і т.п. Записую видалені в новий список
//                });

//                removedWords = removedWords.Distinct().ToList(); // видаляю повтори, залишаю список унікальних видалених елементів (винятки із excludedWords )

//                //words.ForEach(x => Console.WriteLine(x));

//                var statisticList = new List<ValueTuple<string, int, float>>();//список кортежів (назва,к-сть входжень, відсотки)

//                bool wordAlreadyExist; // перевірка на те, чи слово вже існує в списку

//                object locker = new object();  // об'єкт-заглушка

//                for (int i = 0; i < words.Count; i++) // перебираю кожне слово даного тексту
//                {
//                    int indexI = i;
//                    lock (words)
//                    {
//                        new Thread(() =>
//                        {
//                            var tupleToAdd = statisticList.FirstOrDefault(x => x.Item1 == words[indexI].ToLower());// шукаю чи кортеж з обраним словом вже існує

//                            if (String.IsNullOrWhiteSpace(tupleToAdd.Item1)) // якщо не існує
//                            {
//                                wordAlreadyExist = false;
//                                tupleToAdd = new ValueTuple<string, int, float>(words[indexI].ToLower(), 1, 0); // створюю новий кортеж, додаю в нього обране слово
//                            }
//                            else
//                            {
//                                wordAlreadyExist = true;
//                            }

//                            for (int j = i + 1; j < (words.Count - 1); j++) // починаючи від наступного від обраного слова і до кінця списку
//                            {
//                                int indexJ = j;
//                                if (tupleToAdd.Item1 == words[indexJ].ToLower()) // якщо слово повторюється
//                                {
//                                    tupleToAdd.Item2 += 1; // додаю його до загальної к-сті
//                                }
//                            }

//                            //tupleToAdd.Item3 = (float)tupleToAdd.Item2 * 100 / totalWordsAmount; // вираховую відсоткове значення слова від загальної к-сті слів
//                            tupleToAdd.Item3 = (float)Math.Round((decimal)tupleToAdd.Item2 * 100 / totalWordsAmount, 3); // вираховую відсоткове значення слова від загальної к-сті слів
//                            if (!wordAlreadyExist) // якщо обраного слова ще не було у списку
//                            {
//                                statisticList.Add(tupleToAdd); // додаю до списку значень
//                            }
//                        })
//                        {
//                            Name = $"Thread {indexI}"
//                        }.Start();

//                        //statisticList = statisticList.OrderByDescending(x => x.Item3).ThenBy(x => x.Item1).ToList();
//                        statisticList = statisticList.OrderByDescending(x => x.Item3).ToList();
//                    }
//                }

//                var uniqueWordsAmount = statisticList.Count + removedWords.Count;
//                return Ok(new ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>(totalWordsAmount, uniqueWordsAmount, statisticList)); // загальна к-сть слів, к-сть унікальних слів, інша ін-фа
//            }
//            catch (Exception e)
//            {
//                return StatusCode(500, e.Message);
//            }
//        }
//    }
//}