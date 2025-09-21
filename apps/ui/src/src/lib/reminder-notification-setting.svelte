<script lang="ts">
  const billReminderSettingTurnOn = async () => {
    let result = await Notification.requestPermission();

    if (result === "granted") {
      setInterval(() => {
        new Notification("You have a new bill payment to make today!", {
          body: "bill [xyz] is due today",
        });
      }, 10000);
    }
  };
</script>

<button on:click={() => billReminderSettingTurnOn()}>Turn on reminders</button>
