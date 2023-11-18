const Express = require("express");
const { postFile } = require("./routes/post-file");
const multer = require("multer");

const upload = multer({ dest: 'files'});

const DI = {
  uploads: upload
};

let app = Express();

let port = +process.env.PORT || 80;

app.get('/', (_, res) => (res.send({ app: 'files service' })))
app.post('/files', upload.single('file'), postFile(DI))

app.listen(port, () => {
  console.log(`app listening on port ${port}`)
})
