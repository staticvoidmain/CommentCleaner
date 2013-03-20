# Comment Cleaner - CommandLine removal of commented code.

========

Originated as a work project which did not get much traction, so I decided to black-box implement my own idea and open-source the result. 

Commented code is a blight on large code bases, so lets kill it.

## Intended Syntax

CommentCleaner.exe --dir=c:\dev --report-style=xml --out=report.xml --verbose
CommentCleaner.exe --dir=c:\dev --kill --report-style=text --out=report.txt

## Future

I would like to open this up to allow language-specific extensions to remove comments from all languages beyond just C#.

Building language profiles to improve statistics based matching would be nice.

Lots to do!