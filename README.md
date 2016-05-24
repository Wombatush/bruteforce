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
				
Developed by Evgeny Nazarov, http://nazarov.com.au/