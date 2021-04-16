# MassTransit #

## Consumer types ##
* Consumer
* Temporary Consumers
* Connect Consumers
* Instance
* Handler

## Consumers ##
* Using `ClearMessageDeserializers()` and `UseRawJsonSerializer();` to receive raw json in `ReceiveEndpoint` config.

## Producers ##

* publish
    * Using `UseRawJsonSerializer()` to publishing raw json data in `Publish` config. Beware that setting making all publish messages as raw json. Consumers should receives raw json only otherwise will failed and moving message to error queues.

## Message ##
* Using contracts project/library.
* MassTransit uses the full type name, including the namespace, for message contracts. When creating the same message type in two separate projects, the namespaces must **match** or the message will not be consumed.
* Commands
    * A command tells a service to do something. Commands are sent (using `Send`) to an endpoint, as it is expected that a single service instance performs the command action. A command should never be published. 
      Commands should be expressed in a `verb-noun` sequence, following the tell style. 
      Example Commands:
        * UpdateCustomerAddress
        * UpgradeCustomerAccount
        * SubmitOrder
    
* Events
    * An event signifies that something has happened. Events are published (using `Publish`) via either IBus (standalone) or ConsumeContext (within a message consumer). An event should not be sent directly to an endpoint. 
      Events should be expressed in a `noun-verb (past tense)` sequence, indicating that something happened. 
      Example Events:
        * CustomerAddressUpdated
        * CustomerAccountUpgraded
        * OrderSubmitted, OrderAccepted, OrderRejected, OrderShipped
    

