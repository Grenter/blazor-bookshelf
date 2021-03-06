﻿using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using BookShelf.Lambda.Util;
using BookShelf.Shared.Enums;
using BookShelf.Shared.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BookShelf.Lambda
{
    public class BookFunction
    {
        private readonly IList<Book> _keepList = new List<Book>
        {
            new Book
            {
                Authors = "Kent Beck",
                CoverImage = "https://images-na.ssl-images-amazon.com/images/I/51kDbV%2BN65L._SX396_BO1,204,203,200_.jpg",
                Format = Format.Paperback,
                Genre = "Computer Science > Technical",
                Id = "80fb277a-049f-44f6-9f5a-7757dd8388d9",
                Shelf = "Read",
                Title = "Test Driven Development: By Example",
                YearRead = 2018
            },
            new Book
            {
                Authors = "Robert Martin",
                CoverImage = "https://images.gr-assets.com/books/1436202607l/3735293.jpg",
                Format = Format.Kindle,
                Genre = "Computer Science > Technical",
                Id = "e13f4d30-6318-4c9d-b49c-f92edc473fcf",
                Shelf = "Read",
                Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
                YearRead = 2012
            }
        };

        public APIGatewayProxyResponse GetBooks(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var dbContext = DynamoDbUtil.BuildContext();

            var booksSerch = dbContext.ScanAsync<Book>(new List<ScanCondition>()).GetRemainingAsync();

            var books = booksSerch.Result;

            return ResponseBuilder.Http200(JsonConvert.SerializeObject(books), new Dictionary<string, string> { { "Content-Type", "application/json" } });
        }

        public APIGatewayProxyResponse ResetBooks(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var dbContext = DynamoDbUtil.BuildContext();

            var booksSerch = dbContext.ScanAsync<Book>(new List<ScanCondition>()).GetRemainingAsync();

            var books = booksSerch.Result;

            foreach (var book in books)
            {
                dbContext.DeleteAsync(book).Wait();
            }

            foreach (var book in _keepList)
            {
                dbContext.SaveAsync(book).Wait();
            }

            return ResponseBuilder.Http200(JsonConvert.SerializeObject(true));
        }

        public APIGatewayProxyResponse PostBook(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var book = JsonConvert.DeserializeObject<Book>(request.Body);

            book.Id = Guid.NewGuid().ToString();

            var dbContext = DynamoDbUtil.BuildContext();

            dbContext.SaveAsync(book).Wait();

            var reponse = ResponseBuilder.Http200(JsonConvert.SerializeObject(book.Id));

            return reponse;
        }

        public APIGatewayProxyResponse DeleteBook(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var qryParams = request.QueryStringParameters;

            context.Logger.LogLine(string.Join(',', qryParams.Keys));

            var dbContext = DynamoDbUtil.BuildContext();

            var bookData = dbContext.LoadAsync<Book>(qryParams["bookId"]);

            var book = bookData.Result;

            dbContext.DeleteAsync(book).Wait();

            return ResponseBuilder.Http200(JsonConvert.SerializeObject(true));
        }

        public APIGatewayProxyResponse PutBook(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var qryParams = request.QueryStringParameters;

            context.Logger.LogLine(string.Join(',', qryParams.Values));

            var dbContext = DynamoDbUtil.BuildContext();

            var existingBook = dbContext.LoadAsync<Book>(qryParams["bookId"]).Result;

            var updateBook = JsonConvert.DeserializeObject<Book>(request.Body);

            existingBook.Authors = updateBook.Authors;
            existingBook.Genre = updateBook.Genre;
            existingBook.Title = updateBook.Title;
            existingBook.YearRead = updateBook.YearRead;
            existingBook.Shelf = updateBook.Shelf;
            existingBook.CoverImage = updateBook.CoverImage;
            existingBook.Format = updateBook.Format;

            dbContext.SaveAsync(existingBook).Wait();

            return ResponseBuilder.Http200(JsonConvert.SerializeObject(existingBook));
        }
    }
}