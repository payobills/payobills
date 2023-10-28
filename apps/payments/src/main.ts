import { NestFactory } from '@nestjs/core';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import { AppModule } from './app/app.module';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  const config = new DocumentBuilder()
    .setTitle('@payobills/payments')
    .setDescription('APIs to help record user payments for bills')
    .setVersion('1.0')
    .addTag('payments')
    .build();
  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup('swagger', app, document);

  await app.listen(process.env.PORT || 80, '0.0.0.0');
  console.log(`service running at ${await app.getUrl()}`);
}

bootstrap();
