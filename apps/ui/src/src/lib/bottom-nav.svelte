<script>
    import Icon from "svelte-awesome";
    import { faBars } from "@fortawesome/free-solid-svg-icons";
    import { goto } from "$app/navigation";
  </script>
  
  <nav>
    <p><span class='title'>coming soon...</span></p>
  </nav>
  
  <style>
    nav {
      background-color: var(--primary-color);
      display: flex;
      align-items: center;
      padding: 0;
      margin: 0;
      height: 4rem;
    }
    p{  text-align: center;}
    .title {
      font-size: 1.2rem;
      text-transform: unset;
      padding: 0;
      color: white;
    
    }
  </style>
  