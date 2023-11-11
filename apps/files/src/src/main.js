import Express from "express";

let app = Express();

app.listen(3000)

let port = +process.env.PORT || 80;

app.get('/', (_, res) => (res.send({ app: 'files service' })))

app.listen(port, () => {
    console.log(`app listening on port ${port}`)
})
