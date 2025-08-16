import { sveltekit } from '@sveltejs/kit/vite';
import { svelteInspector } from '@sveltejs/vite-plugin-svelte-inspector';
import type { UserConfig } from 'vite';

const config: UserConfig = {
	plugins: [sveltekit(), svelteInspector({})],
	base: '/',
	optimizeDeps: {
		exclude: ['@urql/svelte'],
	},
	server: {
		proxy: {
			// https://stackoverflow.com/questions/64677212/how-to-configure-proxy-in-vite
			'/gateway/graphql': `${process.env.GATEWAY}/graphql`,
			'/files': {
				target: process.env.FILES_SERVICE,
				rewrite: (path) => path.replace(/^\/files/, ''),
			},
		}
	}
};

export default config;
