import adapterNode from '@sveltejs/adapter-node';
import adapterStatic from '@sveltejs/adapter-static';
import { sveltePreprocess } from 'svelte-preprocess';
import path from 'path';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://github.com/sveltejs/svelte-preprocess
	// for more information about preprocessors
	preprocess: sveltePreprocess(),
	kit: {
		alias: {
			'$utils': path.resolve('./src/utils'),
		},
		adapter: process.env.BUILD__MODE === 'STATIC' ? adapterStatic({ fallback: "index.html" }) : adapterNode(),
	}
};

export default config;
