# Dapr
A little playground around dapr possibilities.

https://dapr.io/

Based on a set of APIs which each manages a very specific part of a complex business. This can be seen as a basic domain separated on few contexts from a Domain Driven Design perspective.
Let's see how dapr can help us to make our APIs communicate with each other to animate these atomic sets in a coherent business logic. 

# Init
dapr init
dapr run --app-id playgrounddapr --dapr-http-port 3500 --components-path ./eng/components
