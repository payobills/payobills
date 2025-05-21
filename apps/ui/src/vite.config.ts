import { sveltekit } from '@sveltejs/kit/vite';
import type { UserConfig } from 'vite';

const config: UserConfig = {
	plugins: [sveltekit()],
	base: '/',
	optimizeDeps: {
		exclude: ['@urql/svelte'],
	},
	server: {
		proxy: {
			// https://stackoverflow.com/questions/64677212/how-to-configure-proxy-in-vite
			'/gateway/graphql': `${process.env.GATEWAY}/graphql`,
			'/files': process.env.MODE === 'local' ? `${process.env.FILES_SERVICE}` : `${process.env.GATEWAY}`,
		}
	}
};

export default config;
