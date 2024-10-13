var { Connection } = require('amqplib-as-promised');

module.exports = {
    generate: async () => {
        try {
            const connection = new Connection(process.env.EVENT_QUEUE_CONNECTION_STRING)
            await connection.init()
            const channel = await connection.createChannel()

            return channel;
        }
        catch (error) { throw error; }
    }
}
