﻿using System;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using BookShelf.Backend.Lambda.Util;
using Newtonsoft.Json;
using System.Collections.Generic;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace BookShelf.Backend.Lambda
{
    public class ShelfFunction
    {
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var qryParams = request.QueryStringParameters;

            context.Logger.LogLine(string.Join(',', qryParams.Keys));

            var dbContext = DynamoDbUtil.BuildContext();

            var shelfId = qryParams["shelfId"];

            var shelf = dbContext.LoadAsync<Model.BookShelf>(shelfId);

            return ResponseBuilder.Http200(JsonConvert.SerializeObject(shelf.Result), new Dictionary<string, string> { { "Content-Type", "application/json" } });
        }

        public APIGatewayProxyResponse Post(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var bookShelf = JsonConvert.DeserializeObject<Model.BookShelf>(request.Body);

            bookShelf.BookShelfId = Guid.NewGuid().ToString();

            var dbContext = DynamoDbUtil.BuildContext();

            dbContext.SaveAsync(bookShelf).Wait();

            var reponse = ResponseBuilder.Http200(bookShelf.BookShelfId);

            return reponse;
        }
    }
}
