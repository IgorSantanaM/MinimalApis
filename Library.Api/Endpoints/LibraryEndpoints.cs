﻿using FluentValidation.Results;
using FluentValidation;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Cors;
using Library.Api.Endpoints.Internal;

namespace Library.Api.Endpoints
{
    public class LibraryEndpoints : IEndpoints
    {

        private const string ContentType = "application/json";
        private const string Tag = "Books";
        private const string BaseRoute = "books";
        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IBookService, BookService>();
        }

        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost(BaseRoute, CreateBookAsync)
               .WithName("CreateBook")
               .Accepts<Book>(ContentType)
               .Produces<Book>(201)
               .Produces<IEnumerable<ValidationFailure>>(400)
               .WithTags(Tag);

            app.MapGet(BaseRoute, GetAllBooksAsync)
                .WithName("GetBooks")
                .Produces<IEnumerable<Book>>(200)
                .WithTags(Tag);

            app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbnAsync).WithName("GetBook")
              .Produces<Book>(200)
              .Produces(404)
              .WithTags(Tag);


            app.MapPut($"{BaseRoute}/{{isbn}}",UpdateBookAsync).WithName("UpdateBook")
              .Accepts<Book>(ContentType)
              .Produces<Book>(201)
              .Produces<IEnumerable<ValidationFailure>>(400)
              .WithTags(Tag);

            app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync).WithName("DeleteBook")
              .Produces(204)
              .Produces(404)
              .WithTags(Tag);

            app.MapGet("status", ApiStatusAsync);
        }
        internal static async Task<IResult> CreateBookAsync(Book book, IBookService bookService, LinkGenerator linker,
                                            IValidator<Book> validator, HttpContext context)
        {
            var validationResult = await validator.ValidateAsync(book);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            var created = await bookService.CreateAsync(book);
            if (!created)
            {
                return Results.BadRequest(new List<ValidationFailure>
                {
                    new("Isbn", "A book with this ISBN-13 already exists")
                });
            }

            //var path = linker.GetPathByName("GetBook", new { Isbn = book.Isbn })!;
            var locationUri = linker.GetUriByName(context, "GetBook", new { Isbn = book.Isbn })!;
            return Results.Created(locationUri, book);
            //return Results.CreatedAtRoute("GetBook", new {Isbn = book.Isbn}, book);   
        }

        internal static async Task<IResult> GetAllBooksAsync(IBookService bookService, string? searchTerm)
        {
            if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
            {
                var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
                return Results.Ok(matchedBooks);
            }

            var books = await bookService.GetAllAsync();
            return Results.Ok(books);
        }

        internal static async Task<IResult> GetBookByIsbnAsync(string isbn, IBookService bookService)
        {
            var book = await bookService.GetByIsbnAsync(isbn);
            return book is not null ? Results.Ok(book) : Results.NotFound();
        }

        internal static async Task<IResult> UpdateBookAsync(string isbn, Book book, IBookService bookService, IValidator<Book> validator)
        {
            book.Isbn = isbn;
            var validationResult = await validator.ValidateAsync(book);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }

            var updated = await bookService.UpdateAsync(book);
            return updated ? Results.Ok(book) : Results.NotFound();
        }

        internal static async Task<IResult> DeleteBookAsync(string isbn, IBookService bookService)
        {
            var deleted = await bookService.DeleteAsync(isbn);
            return deleted ? Results.NoContent() : Results.NotFound();
        }

        internal static IResult ApiStatusAsync()
        {
            return Results.Extensions.Html(@"
                <!DOCTYPE html>
                    <html lang=""en"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Document</title>
                        </head>
                        <body>
                            <h1>Hello World!!! Api is Running!</h1>
                        </body>
                   </html>");
        }
    }
}
