# Azure Chronos

Azure Chronos is a robust solution that leverages Azure Durable Functions, C#, and .NET 6.0 to facilitate automated lifecycle management of Azure virtual machines based on user-defined CRON schedules through Azure Tags. This sophisticated yet easy-to-use tool reduces the need for manual intervention in recurring VM management tasks.  
Applying tags to a virtual machine provides a simple, code-free approach to scheduling, eliminating the need for complex tool configurations or coding, and making VM management much more efficient and accessible.

### Benefits
Implementing Azure Chronos reduces operational costs and optimises resource utilisation by strategically releasing Azure virtual machines during off-peak periods. It provides granular control over system uptime and ensures that virtual machines are only running when they are needed, resulting in significant cost savings, improved performance, and elimination of unnecessary resource consumption.

## Tags
| Azure Tag   |      Examples      |  Description |
|----------|-------------|:------|
| AzChronos_Startup | `0 2 * * 4` | This tag contains the CRON expression that defines the virtual machine's scheduled lifecycle, providing flexibility and precise control over VM operations. |
| AzChronos_Deallocate | `0 2 * * 4` | This tag contains the CRON expression that defines the virtual machine's deallocate event, providing flexibility and precise control over VM operations. |
| AzChronos_Downtime | `1`, `3` | This tag captures the desired downtime for the virtual machine in hours, enabling fine-grained control over downtime and improving cost efficiency by minimising unnecessary uptime. |
| AzChronos_Timezone |    `Central European Time`, `Pacific Standard Time`   |   This tag is used to determine the specific time zone in which the CRON schedule operates, ensuring accuracy and consistency across different geographical locations. |
| AzChronos_Exclusion | `true` |    This tag provides the functionality to exclude a specific VM from the scheduling process, a handy feature that eliminates the need to remove all tags if a VM is to be excluded from the default schedule. |

## Components
Work in progress.

## Getting started
Work in progress.
