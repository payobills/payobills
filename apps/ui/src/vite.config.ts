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
			'/bills-graphql/graphql': process.env.MODE === 'local' ? `${process.env.BILLS_SERVICE}/graphql` : `${process.env.GATEWAY}/bills-graphql/graphql`,
			'/payments-graphql/graphql': process.env.MODE === 'local' ? `${process.env.PAYMENTS_SERVICE}/graphql` : `${process.env.GATEWAY}/payments-graphql/graphql`,
		}
	}
};

export default config;
