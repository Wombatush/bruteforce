# bruteforce

A combinatory test helper with a fancy memorable syntax:

```
Combinations
	.Compose(x => new
	{
		Greeting = x.Only("Hello", "Howdy", "GDay"),
		Participant = x.Only("John", "James", "Bob")
	})
	.RunInParallel(test =>
	{
		Console.WriteLine("{0}, {1}", test.Greeting, test.Participant);
	});
```

The code block above produces following output:

```		
Hello, John
GDay, James
GDay, Bob
Howdy, John
Howdy, Bob
GDay, John
Hello, Bob
Hello, James
Howdy, James
```	

Developed by Evgeny Nazarov, http://nazarov.com.au/