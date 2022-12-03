using AsposeTestingApp.Controllers.APIControllers;
using AsposeTestingApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AsposeTestingAppUnitTests
{
    public class InitialControllerTests
    {
        readonly InitialController _controller = null;
        private readonly AnalyzeViewModel _testData = null;
      
       public InitialControllerTests()
        {
            _controller = new InitialController();
            _testData = new AnalyzeViewModel
            {
                Text = "Split PDF document online is a web service that allows you to split your PDF document into separate pages. This simple application has several modes of operation, you can split your PDF document into separate pages, i.e. each page of the original document will be a separate PDF document, you can split your document into even and odd pages, this function will come in handy if you need to print a document in the form of a book, you can also specify page numbers in the settings and the Split PDF application will create separate PDF documents only with these pages and the fourth mode of operation allows you to create a new PDF document in which there will be only those pages that you specified. ",
                WordsToSplit = 1
            };
        }
        [Fact]
        public async Task CheckAllFinalNumbersTest()
        {
            // Arrange
            int expectedWordsAmount = 127;
            int expectedUniqueWordsAmount = 61;
            int statisticsWordsAmount = 57;// 57 - слів у статистиці

            // Act
            var result = await _controller.AnalyzeTextAsync(_testData);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result); // перевірка на результат
            var resultData = result.Result as OkObjectResult;

            Assert.IsType<ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>>(resultData.Value); // чи значення має вказаний тип
            var resultDataValue = (ValueTuple<int, int, IEnumerable<ValueTuple<string, int, float>>>) resultData.Value;
            var analyzedWordList = resultDataValue.Item3 as List<ValueTuple<string, int, float>>;
            Assert.Equal(expectedWordsAmount, resultDataValue.Item1); //загальна к-сть слів
            Assert.Equal(expectedUniqueWordsAmount, resultDataValue.Item2); //к-сть унікальних слів
            Assert.Equal(statisticsWordsAmount, analyzedWordList.Count); //к-сть слів для порівняння
        }
    }
}
