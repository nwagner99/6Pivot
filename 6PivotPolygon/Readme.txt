Classes:

Shape: an abstract class that defines the individual parameters for each shape.
Measurement: defines a measurement parameter and its value.  e.g. Height.  Also parses the parameter clauses in the input statement.
ShapeRequest: parses the input from the user and returns an object descended from Shape.
JSONShape: contains the properties required to draw each shape on the client.
Individual shapes: implements the Emit function that creates the JSONShape object for the client.

Future improvements:
	Tidy up the error messages to make them more consistent - especially in respct of capitalisation and syntax.
	Consider a different class hierarchy to try and simplify the parsing.  Consider using a lexical analyser.

