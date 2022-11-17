
// this file is generated — do not edit it


/// <reference types="@sveltejs/kit" />

/**
 * Environment variables [loaded by Vite](https://vitejs.dev/guide/env-and-mode.html#env-files) from `.env` files and `process.env`. Like [`$env/dynamic/private`](https://kit.svelte.dev/docs/modules#$env-dynamic-private), this module cannot be imported into public-facing code. This module only includes variables that _do not_ begin with [`config.kit.env.publicPrefix`](https://kit.svelte.dev/docs/configuration#env).
 * 
 * _Unlike_ [`$env/dynamic/private`](https://kit.svelte.dev/docs/modules#$env-dynamic-private), the values exported from this module are statically injected into your bundle at build time, enabling optimisations like dead code elimination.
 * 
 * ```ts
 * import { API_KEY } from '$env/static/private';
 * ```
 * 
 * Note that all environment variables referenced in your code should be declared (for example in an `.env` file), even if they don't have a value until the app is deployed:
 * 
 * ```
 * MY_FEATURE_FLAG=""
 * ```
 * 
 * You can override `.env` values from the command line like so:
 * 
 * ```bash
 * MY_FEATURE_FLAG="enabled" npm run dev
 * ```
 */
declare module '$env/static/private' {
	export const PYENV_VIRTUALENV_INIT: string;
	export const CONDA_PROMPT_MODIFIER: string;
	export const npm_package_dev: string;
	export const LANGUAGE: string;
	export const USER: string;
	export const MYSQL_SERVER_VERSION: string;
	export const npm_config_user_agent: string;
	export const XDG_SESSION_TYPE: string;
	export const GIT_ASKPASS: string;
	export const npm_node_execpath: string;
	export const npm_package_resolved: string;
	export const SHLVL: string;
	export const POWERLINE_COMMAND: string;
	export const TFENV_ROOT: string;
	export const npm_config_noproxy: string;
	export const HOME: string;
	export const OLDPWD: string;
	export const CHROME_DESKTOP: string;
	export const npm_package_optional: string;
	export const DESKTOP_SESSION: string;
	export const TERM_PROGRAM_VERSION: string;
	export const npm_package_json: string;
	export const PYENV_SHELL: string;
	export const npm_package_engines_node: string;
	export const GTK_MODULES: string;
	export const GNOME_SHELL_SESSION_MODE: string;
	export const DIRENV_DIFF: string;
	export const VSCODE_GIT_ASKPASS_MAIN: string;
	export const CLI_CONFIG_TOOLS_LOCATION: string;
	export const VSCODE_GIT_ASKPASS_NODE: string;
	export const npm_config_userconfig: string;
	export const npm_config_local_prefix: string;
	export const npm_package_integrity: string;
	export const SYSTEMD_EXEC_PID: string;
	export const DBUS_SESSION_BUS_ADDRESS: string;
	export const GVM_DIR: string;
	export const DOTNET_VERSION: string;
	export const npm_config_engine_strict: string;
	export const COLORTERM: string;
	export const COLOR: string;
	export const NVM_DIR: string;
	export const npm_config_metrics_registry: string;
	export const LOGNAME: string;
	export const _: string;
	export const npm_config_prefix: string;
	export const XDG_SESSION_CLASS: string;
	export const USER_ZDOTDIR: string;
	export const USERNAME: string;
	export const TERM: string;
	export const npm_config_cache: string;
	export const GNOME_DESKTOP_SESSION_ID: string;
	export const AUTOJUMP_ERROR_PATH: string;
	export const DOTNET_ROOT: string;
	export const WINDOWPATH: string;
	export const CLI_CONFIG_CONF_LOCATION: string;
	export const npm_config_node_gyp: string;
	export const PATH: string;
	export const SESSION_MANAGER: string;
	export const DIRENV_WATCHES: string;
	export const NODE: string;
	export const npm_package_name: string;
	export const GNOME_TERMINAL_SCREEN: string;
	export const XDG_MENU_PREFIX: string;
	export const XDG_RUNTIME_DIR: string;
	export const GDK_BACKEND: string;
	export const DISPLAY: string;
	export const LANG: string;
	export const XDG_CURRENT_DESKTOP: string;
	export const VSCODE_INJECTION: string;
	export const XDG_SESSION_DESKTOP: string;
	export const GNOME_TERMINAL_SERVICE: string;
	export const XMODIFIERS: string;
	export const XAUTHORITY: string;
	export const DIRENV_HOME: string;
	export const TERM_PROGRAM: string;
	export const VSCODE_GIT_IPC_HANDLE: string;
	export const npm_lifecycle_script: string;
	export const SSH_AUTH_SOCK: string;
	export const SSH_AGENT_LAUNCHER: string;
	export const ORIGINAL_XDG_CURRENT_DESKTOP: string;
	export const SHELL: string;
	export const BILLS_DB_PATH: string;
	export const npm_package_version: string;
	export const npm_lifecycle_event: string;
	export const QT_ACCESSIBILITY: string;
	export const GDMSESSION: string;
	export const DIRENV_DIR: string;
	export const npm_package_dev_optional: string;
	export const GPG_AGENT_INFO: string;
	export const QT_IM_MODULE: string;
	export const POSH_THEME: string;
	export const VSCODE_GIT_ASKPASS_EXTRA_ARGS: string;
	export const npm_config_globalconfig: string;
	export const npm_config_init_module: string;
	export const npm_package_peer: string;
	export const PWD: string;
	export const npm_execpath: string;
	export const XDG_CONFIG_DIRS: string;
	export const XDG_DATA_DIRS: string;
	export const PYENV_ROOT: string;
	export const ZDOTDIR: string;
	export const npm_config_global_prefix: string;
	export const AUTOJUMP_SOURCED: string;
	export const npm_command: string;
	export const VTE_VERSION: string;
	export const CLI_CONFIG_PROGRAMS_CONF: string;
	export const INIT_CWD: string;
	export const EDITOR: string;
}

/**
 * Similar to [`$env/static/private`](https://kit.svelte.dev/docs/modules#$env-static-private), except that it only includes environment variables that begin with [`config.kit.env.publicPrefix`](https://kit.svelte.dev/docs/configuration#env) (which defaults to `PUBLIC_`), and can therefore safely be exposed to client-side code.
 * 
 * Values are replaced statically at build time.
 * 
 * ```ts
 * import { PUBLIC_BASE_URL } from '$env/static/public';
 * ```
 */
declare module '$env/static/public' {

}

/**
 * This module provides access to runtime environment variables, as defined by the platform you're running on. For example if you're using [`adapter-node`](https://github.com/sveltejs/kit/tree/master/packages/adapter-node) (or running [`vite preview`](https://kit.svelte.dev/docs/cli)), this is equivalent to `process.env`. This module only includes variables that _do not_ begin with [`config.kit.env.publicPrefix`](https://kit.svelte.dev/docs/configuration#env).
 * 
 * This module cannot be imported into public-facing code.
 * 
 * ```ts
 * import { env } from '$env/dynamic/private';
 * console.log(env.DEPLOYMENT_SPECIFIC_VARIABLE);
 * ```
 * 
 * > In `dev`, `$env/dynamic` always includes environment variables from `.env`. In `prod`, this behavior will depend on your adapter.
 */
declare module '$env/dynamic/private' {
	export const env: {
		PYENV_VIRTUALENV_INIT: string;
		CONDA_PROMPT_MODIFIER: string;
		npm_package_dev: string;
		LANGUAGE: string;
		USER: string;
		MYSQL_SERVER_VERSION: string;
		npm_config_user_agent: string;
		XDG_SESSION_TYPE: string;
		GIT_ASKPASS: string;
		npm_node_execpath: string;
		npm_package_resolved: string;
		SHLVL: string;
		POWERLINE_COMMAND: string;
		TFENV_ROOT: string;
		npm_config_noproxy: string;
		HOME: string;
		OLDPWD: string;
		CHROME_DESKTOP: string;
		npm_package_optional: string;
		DESKTOP_SESSION: string;
		TERM_PROGRAM_VERSION: string;
		npm_package_json: string;
		PYENV_SHELL: string;
		npm_package_engines_node: string;
		GTK_MODULES: string;
		GNOME_SHELL_SESSION_MODE: string;
		DIRENV_DIFF: string;
		VSCODE_GIT_ASKPASS_MAIN: string;
		CLI_CONFIG_TOOLS_LOCATION: string;
		VSCODE_GIT_ASKPASS_NODE: string;
		npm_config_userconfig: string;
		npm_config_local_prefix: string;
		npm_package_integrity: string;
		SYSTEMD_EXEC_PID: string;
		DBUS_SESSION_BUS_ADDRESS: string;
		GVM_DIR: string;
		DOTNET_VERSION: string;
		npm_config_engine_strict: string;
		COLORTERM: string;
		COLOR: string;
		NVM_DIR: string;
		npm_config_metrics_registry: string;
		LOGNAME: string;
		_: string;
		npm_config_prefix: string;
		XDG_SESSION_CLASS: string;
		USER_ZDOTDIR: string;
		USERNAME: string;
		TERM: string;
		npm_config_cache: string;
		GNOME_DESKTOP_SESSION_ID: string;
		AUTOJUMP_ERROR_PATH: string;
		DOTNET_ROOT: string;
		WINDOWPATH: string;
		CLI_CONFIG_CONF_LOCATION: string;
		npm_config_node_gyp: string;
		PATH: string;
		SESSION_MANAGER: string;
		DIRENV_WATCHES: string;
		NODE: string;
		npm_package_name: string;
		GNOME_TERMINAL_SCREEN: string;
		XDG_MENU_PREFIX: string;
		XDG_RUNTIME_DIR: string;
		GDK_BACKEND: string;
		DISPLAY: string;
		LANG: string;
		XDG_CURRENT_DESKTOP: string;
		VSCODE_INJECTION: string;
		XDG_SESSION_DESKTOP: string;
		GNOME_TERMINAL_SERVICE: string;
		XMODIFIERS: string;
		XAUTHORITY: string;
		DIRENV_HOME: string;
		TERM_PROGRAM: string;
		VSCODE_GIT_IPC_HANDLE: string;
		npm_lifecycle_script: string;
		SSH_AUTH_SOCK: string;
		SSH_AGENT_LAUNCHER: string;
		ORIGINAL_XDG_CURRENT_DESKTOP: string;
		SHELL: string;
		BILLS_DB_PATH: string;
		npm_package_version: string;
		npm_lifecycle_event: string;
		QT_ACCESSIBILITY: string;
		GDMSESSION: string;
		DIRENV_DIR: string;
		npm_package_dev_optional: string;
		GPG_AGENT_INFO: string;
		QT_IM_MODULE: string;
		POSH_THEME: string;
		VSCODE_GIT_ASKPASS_EXTRA_ARGS: string;
		npm_config_globalconfig: string;
		npm_config_init_module: string;
		npm_package_peer: string;
		PWD: string;
		npm_execpath: string;
		XDG_CONFIG_DIRS: string;
		XDG_DATA_DIRS: string;
		PYENV_ROOT: string;
		ZDOTDIR: string;
		npm_config_global_prefix: string;
		AUTOJUMP_SOURCED: string;
		npm_command: string;
		VTE_VERSION: string;
		CLI_CONFIG_PROGRAMS_CONF: string;
		INIT_CWD: string;
		EDITOR: string;
		[key: string]: string | undefined;
	}
}

/**
 * Similar to [`$env/dynamic/private`](https://kit.svelte.dev/docs/modules#$env-dynamic-private), but only includes variables that begin with [`config.kit.env.publicPrefix`](https://kit.svelte.dev/docs/configuration#env) (which defaults to `PUBLIC_`), and can therefore safely be exposed to client-side code.
 * 
 * Note that public dynamic environment variables must all be sent from the server to the client, causing larger network requests — when possible, use `$env/static/public` instead.
 * 
 * ```ts
 * import { env } from '$env/dynamic/public';
 * console.log(env.PUBLIC_DEPLOYMENT_SPECIFIC_VARIABLE);
 * ```
 */
declare module '$env/dynamic/public' {
	export const env: {
		[key: string]: string | undefined;
	}
}
