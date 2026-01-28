const Minio = require('minio');

export default {
  generate: () => {
    var minioClient = new Minio.Client({
      endPoint: process.env.STORAGE__ENDPOINT,
      port: +process.env.STORAGE__PORT,
      useSSL: process.env.STORAGE__USE_SSL === 'true',
      accessKey: process.env.STORAGE__ACCESS_KEY,
      secretKey: process.env.STORAGE__SECRET_KEY,
    });

    return minioClient;
  },
};

