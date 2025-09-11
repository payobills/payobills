# payobills

A self-hosted (Bring and Keep your own Data) personal bill management app. The full business intent of this project is documented in the [business intent document](docs/business-intent.md).

## Screenshots
You can find the [Figma Designs](https://www.figma.com/design/RuZ5khlNpA4IgKPb1iebzc/payobills-main) for this app. Feel free to connect with me if you're a designer and want to contribute!
Note: I'm not a designer, these designs have been generated with GenAI.

| <img src="docs/images/Overview Page.png" alt="Screenshot of the Overview Page" > | <img src="docs/images/Bills Page.png" alt="Screenshot of all your bills" > | 
| --- | --- |

| <img src="docs/images/Bill Detail Page.png" alt="Screenshot of details for one bill" > | <img src="docs/images/Transaction Detail Page.png" alt="Screenshot of details of a transaction" > | 
| --- | --- |

| <img src="docs/images/Settings Page.png" alt="Screenshot of the settings page" > |

## Why create such an app?
- Help me with all my bill related needs; save time and pay bills together.
- Analytics from all my financial data, without exposing my data.
- Learning Kubernetes and tooling across the stack
- Contributing to the Self-Hosted open source community

## Project structure
- Each microservice in a folder in `apps` directory
- A build system to help managing deployment and running the services - uses [Please.Build](https://please.build) by [@thought-machine](https://github.com/thought-machine)
- Code reuse with packages

## Deployment
This project uses a sibling project I created [@mrsauravsahu/kube-homelab](https://github.com/mrsauravsahu/kube-homelab) to create a Kubernetes Cluster using Raspberry Pi nodes - more details on my [YouTube channel @mrsauravsahuin](https://www.youtube.com/watch?v=LfBcERF6qw4)

<img src="docs/images/icon.png" alt="Screenshot of the current version of the App" style="width: 5rem">
