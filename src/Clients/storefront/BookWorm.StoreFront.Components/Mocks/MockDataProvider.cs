using BookWorm.StoreFront.Components.Models;

namespace BookWorm.StoreFront.Components.Mocks;

public static class MockDataProvider
{
    public static List<Book> GetAllBooks()
    {
        return
        [
            new Book(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "The Great Gatsby",
                @"The Great Gatsby is a 1925 novel by American writer F. Scott Fitzgerald. Set in the Jazz Age on Long Island, the novel depicts narrator Nick Carraway's interactions with mysterious millionaire Jay Gatsby and Gatsby's obsession to reunite with his former lover, Daisy Buchanan.

A masterpiece of American literature, this timeless story of wealth, love, and tragedy has captivated readers for generations. The novel explores themes of decadence, idealism, social upheaval, and excess, creating a portrait of the Jazz Age that has been described as a cautionary tale regarding the American Dream.

Key Features:
• Classic American literature
• Explores themes of wealth and class
• Set in the roaring 1920s
• Timeless tale of love and loss
• Essential reading for literature enthusiasts

Perfect for book clubs, students, and anyone interested in American classics.",
                "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400",
                15.99m,
                null,
                "InStock",
                new Category(
                    Guid.Parse("00000001-0001-0001-0001-000000000001"),
                    "Classic Literature"
                ),
                new Publisher(Guid.Parse("00000001-0002-0002-0002-000000000001"), "Scribner"),
                [
                    new Author(
                        Guid.Parse("00000001-0003-0003-0003-000000000001"),
                        "F. Scott Fitzgerald"
                    ),
                ],
                4.5,
                2180
            ),
            new Book(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "To Kill a Mockingbird",
                @"Harper Lee's Pulitzer Prize-winning masterwork of honor and injustice in the deep South—and the heroism of one man in the face of blind and violent hatred.

One of the most cherished stories of all time, To Kill a Mockingbird has been translated into more than forty languages, sold more than forty million copies worldwide, served as the basis for an enormously popular motion picture, and was voted one of the best novels of the twentieth century by librarians across the country.

The unforgettable novel of a childhood in a sleepy Southern town and the crisis of conscience that rocked it. 'To Kill A Mockingbird' became both an instant bestseller and a critical success when it was first published in 1960.

Key Features:
• Pulitzer Prize winner
• Essential American classic
• Timeless themes of justice and morality
• Beloved by readers of all ages
• Required reading in many schools",
                "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=400",
                14.99m,
                null,
                "InStock",
                new Category(
                    Guid.Parse("00000001-0001-0001-0001-000000000001"),
                    "Classic Literature"
                ),
                new Publisher(Guid.Parse("00000002-0002-0002-0002-000000000002"), "HarperCollins"),
                [new Author(Guid.Parse("00000002-0003-0003-0003-000000000002"), "Harper Lee")],
                4.8,
                3542
            ),
            new Book(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "1984",
                @"Among the seminal texts of the 20th century, Nineteen Eighty-Four is a rare work that grows more haunting as its futuristic purgatory becomes more real. Published in 1949, the book offers political satirist George Orwell's nightmare vision of a totalitarian, bureaucratic world and one poor stiff's attempt to find individuality.

The brilliance of the novel is Orwell's prescience of modern life—the ubiquity of television, the distortion of the language—and his ability to construct such a thorough version of hell. Required reading for students since it was published, it ranks among the most terrifying novels ever written.

Key Features:
• Dystopian masterpiece
• Prophetic vision of surveillance state
• Essential political commentary
• Timeless and relevant
• Cultural touchstone",
                "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400",
                13.99m,
                null,
                "OutOfStock",
                new Category(Guid.Parse("00000002-0001-0001-0001-000000000002"), "Science Fiction"),
                new Publisher(Guid.Parse("00000003-0002-0002-0002-000000000003"), "Signet Classic"),
                [new Author(Guid.Parse("00000003-0003-0003-0003-000000000003"), "George Orwell")],
                4.7,
                4219
            ),
            new Book(
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                "Pride and Prejudice",
                @"Since its immediate success in 1813, Pride and Prejudice has remained one of the most popular novels in the English language. Jane Austen called this brilliant work 'her own darling child' and its vivacious heroine, Elizabeth Bennet, 'as delightful a creature as ever appeared in print.'

The romantic clash between the opinionated Elizabeth and her proud beau, Mr. Darcy, is a splendid performance of civilized sparring. And Jane Austen's radiant wit sparkles as her characters dance a delicate quadrille of flirtation and intrigue, making this book the most superb comedy of manners of Regency England.

Key Features:
• Timeless romance classic
• Witty social commentary
• Memorable characters
• Perfect for romance readers
• Beloved worldwide",
                "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400",
                12.99m,
                16.99m,
                "InStock",
                new Category(Guid.Parse("00000003-0001-0001-0001-000000000003"), "Romance"),
                new Publisher(
                    Guid.Parse("00000004-0002-0002-0002-000000000004"),
                    "Penguin Classics"
                ),
                [new Author(Guid.Parse("00000004-0003-0003-0003-000000000004"), "Jane Austen")],
                4.6,
                5324
            ),
            new Book(
                Guid.Parse("55555555-5555-5555-5555-555555555555"),
                "The Hobbit",
                @"The Hobbit is a tale of high adventure, undertaken by a company of dwarves in search of dragon-guarded gold. A reluctant partner in this perilous quest is Bilbo Baggins, a comfort-loving unambitious hobbit, who surprises even himself by his resourcefulness and skill as a burglar.

Encounters with trolls, goblins, dwarves, elves, and giant spiders, conversations with the dragon, Smaug, and a rather unwilling presence at the Battle of Five Armies are just some of the adventures that befall Bilbo.

Key Features:
• Fantasy classic
• Prequel to Lord of the Rings
• Adventure for all ages
• Rich world-building
• Timeless storytelling",
                "https://images.unsplash.com/photo-1519682337058-a94d519337bc?w=400",
                14.99m,
                null,
                "InStock",
                new Category(Guid.Parse("00000004-0001-0001-0001-000000000004"), "Fantasy"),
                new Publisher(
                    Guid.Parse("00000005-0002-0002-0002-000000000005"),
                    "Houghton Mifflin"
                ),
                [new Author(Guid.Parse("00000005-0003-0003-0003-000000000005"), "J.R.R. Tolkien")],
                4.8,
                6287
            ),
            new Book(
                Guid.Parse("66666666-6666-6666-6666-666666666666"),
                "Harry Potter and the Sorcerer's Stone",
                @"Harry Potter has never even heard of Hogwarts when the letters start dropping on the doormat at number four, Privet Drive. Addressed in green ink on yellowish parchment with a purple seal, they are swiftly confiscated by his grisly aunt and uncle.

Then, on Harry's eleventh birthday, a great beetle-eyed giant of a man called Rubeus Hagrid bursts in with some astonishing news: Harry Potter is a wizard, and he has a place at Hogwarts School of Witchcraft and Wizardry.

An incredible adventure is about to begin!

Key Features:
• International bestseller
• Beginning of beloved series
• Magical adventure
• Perfect for all ages
• Award-winning",
                "https://images.unsplash.com/photo-1621351183012-e2f9972dd9bf?w=400",
                10.99m,
                14.99m,
                "InStock",
                new Category(Guid.Parse("00000004-0001-0001-0001-000000000004"), "Fantasy"),
                new Publisher(Guid.Parse("00000006-0002-0002-0002-000000000006"), "Scholastic"),
                [new Author(Guid.Parse("00000006-0003-0003-0003-000000000006"), "J.K. Rowling")],
                4.9,
                8512
            ),
            new Book(
                Guid.Parse("77777777-7777-7777-7777-777777777777"),
                "The Catcher in the Rye",
                @"The hero-narrator of The Catcher in the Rye is an ancient child of sixteen, a native New Yorker named Holden Caulfield. Through circumstances that tend to preclude adult, secondhand description, he leaves his prep school in Pennsylvania and goes underground in New York City for three days.

The book is a critique of superficiality in society. It is Holden's story, told in Holden's own strange, wonderful language.

Key Features:
• Coming-of-age classic
• Iconic teenage protagonist
• Cultural phenomenon
• Timeless themes
• Essential reading",
                "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400",
                13.99m,
                17.99m,
                "InStock",
                new Category(
                    Guid.Parse("00000001-0001-0001-0001-000000000001"),
                    "Classic Literature"
                ),
                new Publisher(Guid.Parse("00000007-0002-0002-0002-000000000007"), "Little, Brown"),
                [new Author(Guid.Parse("00000007-0003-0003-0003-000000000007"), "J.D. Salinger")],
                4.2,
                3876
            ),
            new Book(
                Guid.Parse("88888888-8888-8888-8888-888888888888"),
                "The Lord of the Rings",
                @"One Ring to rule them all, One Ring to find them, One Ring to bring them all and in the darkness bind them.

In ancient times the Rings of Power were crafted by the Elven-smiths, and Sauron, the Dark Lord, forged the One Ring, filling it with his own power so that he could rule all others. But the One Ring was taken from him, and though he sought it throughout Middle-earth, it remained lost to him.

This complete trilogy includes The Fellowship of the Ring, The Two Towers, and The Return of the King.

Key Features:
• Epic fantasy trilogy
• Complete unabridged edition
• Award-winning masterpiece
• Rich mythology
• Essential fantasy reading",
                "https://images.unsplash.com/photo-1621351183012-e2f9972dd9bf?w=400",
                24.99m,
                29.99m,
                "InStock",
                new Category(Guid.Parse("00000004-0001-0001-0001-000000000004"), "Fantasy"),
                new Publisher(
                    Guid.Parse("00000005-0002-0002-0002-000000000005"),
                    "Houghton Mifflin"
                ),
                [new Author(Guid.Parse("00000005-0003-0003-0003-000000000005"), "J.R.R. Tolkien")],
                4.9,
                9128
            ),
            new Book(
                Guid.Parse("99999999-9999-9999-9999-999999999999"),
                "The Da Vinci Code",
                @"While in Paris, Harvard symbologist Robert Langdon is awakened by a phone call in the dead of the night. The elderly curator of the Louvre has been murdered inside the museum, his body covered in baffling symbols.

As Langdon and gifted French cryptologist Sophie Neveu sort through the bizarre riddles, they are stunned to discover a trail of clues hidden in the works of Leonardo da Vinci—clues visible for all to see and yet ingeniously disguised by the painter.

Key Features:
• International bestseller
• Thrilling mystery
• Art and history combined
• Page-turning suspense
• Perfect for thriller fans",
                "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=400",
                16.99m,
                null,
                "InStock",
                new Category(
                    Guid.Parse("00000005-0001-0001-0001-000000000005"),
                    "Mystery & Thriller"
                ),
                new Publisher(Guid.Parse("00000008-0002-0002-0002-000000000008"), "Doubleday"),
                [new Author(Guid.Parse("00000008-0003-0003-0003-000000000008"), "Dan Brown")],
                4.3,
                7642
            ),
            new Book(
                Guid.Parse("0000000a-000a-000a-000a-00000000000a"),
                "Atomic Habits",
                @"No matter your goals, Atomic Habits offers a proven framework for improving—every day. James Clear, one of the world's leading experts on habit formation, reveals practical strategies that will teach you exactly how to form good habits, break bad ones, and master the tiny behaviors that lead to remarkable results.

If you're having trouble changing your habits, the problem isn't you. The problem is your system. Bad habits repeat themselves again and again not because you don't want to change, but because you have the wrong system for change.

Key Features:
• #1 New York Times bestseller
• Practical self-improvement
• Evidence-based strategies
• Life-changing insights
• Highly recommended",
                "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=400",
                18.99m,
                null,
                "InStock",
                new Category(Guid.Parse("00000006-0001-0001-0001-000000000006"), "Self-Help"),
                new Publisher(Guid.Parse("00000009-0002-0002-0002-000000000009"), "Avery"),
                [new Author(Guid.Parse("00000009-0003-0003-0003-000000000009"), "James Clear")],
                4.7,
                5894
            ),
        ];
    }

    public static List<Book> GetNewArrivals()
    {
        return [.. GetAllBooks().Take(4)];
    }

    public static List<Publisher> GetPublishers()
    {
        return
        [
            new Publisher(Guid.Parse("00000001-0002-0002-0002-000000000001"), "Scribner"),
            new Publisher(Guid.Parse("00000002-0002-0002-0002-000000000002"), "HarperCollins"),
            new Publisher(Guid.Parse("00000003-0002-0002-0002-000000000003"), "Signet Classic"),
            new Publisher(Guid.Parse("00000004-0002-0002-0002-000000000004"), "Penguin Classics"),
            new Publisher(Guid.Parse("00000005-0002-0002-0002-000000000005"), "Houghton Mifflin"),
            new Publisher(Guid.Parse("00000006-0002-0002-0002-000000000006"), "Scholastic"),
            new Publisher(Guid.Parse("00000007-0002-0002-0002-000000000007"), "Little, Brown"),
            new Publisher(Guid.Parse("00000008-0002-0002-0002-000000000008"), "Doubleday"),
            new Publisher(Guid.Parse("00000009-0002-0002-0002-000000000009"), "Avery"),
        ];
    }

    public static List<Category> GetCategories()
    {
        return
        [
            new Category(Guid.Parse("00000001-0001-0001-0001-000000000001"), "Classic Literature"),
            new Category(Guid.Parse("00000002-0001-0001-0001-000000000002"), "Science Fiction"),
            new Category(Guid.Parse("00000003-0001-0001-0001-000000000003"), "Romance"),
            new Category(Guid.Parse("00000004-0001-0001-0001-000000000004"), "Fantasy"),
            new Category(Guid.Parse("00000005-0001-0001-0001-000000000005"), "Mystery"),
            new Category(Guid.Parse("00000006-0001-0001-0001-000000000006"), "Non-Fiction"),
            new Category(Guid.Parse("00000007-0001-0001-0001-000000000007"), "Self-Help"),
        ];
    }

    public static List<Feedback> GetFeedbacksByBookId(Guid bookId)
    {
        return
        [
            new(
                Guid.NewGuid(),
                "Michael",
                "T.",
                "I've been reading this book for about a month now and I'm extremely impressed with the quality of both the writing and the story. The character development is excellent and the plot keeps you engaged throughout.",
                5,
                bookId
            ),
            new(
                Guid.NewGuid(),
                "Sarah",
                "L.",
                "One of the best books I've read this year. The author's style is captivating and the narrative flows beautifully. Couldn't put it down!",
                5,
                bookId
            ),
            new(
                Guid.NewGuid(),
                "James",
                "R.",
                "Overall a solid book with an interesting premise. A few pacing issues in the middle section, but the ending makes up for it. Would still recommend.",
                4,
                bookId
            ),
            new(
                Guid.NewGuid(),
                "Emily",
                "K.",
                "Bought this on a whim and was pleasantly surprised. The depth of the characters and the world-building are phenomenal. Looking forward to more from this author.",
                5,
                bookId
            ),
        ];
    }
}
