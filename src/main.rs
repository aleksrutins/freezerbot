use async_once::AsyncOnce;
use dotenv::dotenv;
use mongodb::{
    bson::doc,
    options::{ClientOptions, FindOneAndUpdateOptions},
    Client, Database,
};
use poise::serenity_prelude::{CreateActionRow, CreateButton};
use std::env;
use tokio::sync::{RwLock, RwLockReadGuard};
mod types;

struct Data {}
type Error = Box<dyn std::error::Error + Send + Sync>;
type Context<'a> = poise::Context<'a, Data, Error>;

async fn connect_db() -> Result<Database, Error> {
    let mut client_options = ClientOptions::parse(env::var("MONGO_URL")?).await?;
    client_options.app_name = Some("freezerbot".into());

    let client = Client::with_options(client_options)?;
    let db = client.database("freezer_data");
    Ok(db)
}

lazy_static::lazy_static! {
    static ref DATABASE: AsyncOnce<RwLock<Database>> = AsyncOnce::new(async {
        RwLock::new(connect_db().await.expect("Failed to connect to database"))
    });
}

fn guild_id(ctx: Context<'_>) -> Option<String> {
    match ctx.guild_id() {
        Some(id) => Some(id.0.to_string()),
        None => None,
    }
}

async fn db<'a>() -> RwLockReadGuard<'a, Database> {
    DATABASE
    .get()
    .await
    .read()
    .await
}

async fn set_frozen(guild_id: String, frozen: bool) -> Result<(), Error> {
    let collection = db().await.collection::<types::FreezeState>("freeze_states");
    collection
        .find_one_and_update(
            doc! {"guild_id": guild_id},
            doc! {"$set": {"frozen": frozen}},
            Some(FindOneAndUpdateOptions::builder().upsert(true).build()),
        )
        .await?;
    Ok(())
}

async fn get_frozen(guild_id: String) -> Result<bool, Error> {
    let collection = db().await.collection::<types::FreezeState>("freeze_states");
    Ok(collection.find_one(doc! {"guild_id": guild_id}, Some(Default::default())).await?.ok_or(anyhow::anyhow!("no guild ID found"))?.frozen)
}


#[poise::command(slash_command)]
async fn freeze(ctx: Context<'_>) -> Result<(), Error> {
    let guild_id = match guild_id(ctx) {
        Some(id) => id,
        None => {
            ctx.say(":bangbang: No guild ID!").await?;
            return Ok(());
        }
    };
    set_frozen(guild_id, true).await?;
    ctx.say(":snowflake: Frozen.").await?;
    Ok(())
}

#[poise::command(slash_command)]
async fn thaw(ctx: Context<'_>) -> Result<(), Error> {
    let guild_id = match guild_id(ctx) {
        Some(id) => id,
        None => {
            ctx.say(":bangbang: No guild ID!").await?;
            return Ok(());
        }
    };
    set_frozen(guild_id, false).await?;
    ctx.say(":sunglasses: Unfrozen.").await?;
    Ok(())
}

#[poise::command(slash_command)]
async fn status(ctx: Context<'_>) -> Result<(), Error> {
    let guild_id = match guild_id(ctx) {
        Some(id) => id,
        None => {
            ctx.say(":bangbang: No guild ID!").await?;
            return Ok(());
        }
    };
    match get_frozen(guild_id).await {
        Ok(frozen) => match frozen {
            true => ctx.say(":snowflake: Your code is currently frozen.").await?,
            false => ctx.say(":sunglasses: Your code is currently unfrozen.").await?
        },
        Err(_) => ctx.say(":bangbang: I'm afraid I'm not tracking your code yet. Try using `/freeze` or `/thaw`.").await?
    };
    Ok(())
}

#[poise::command(slash_command)]
async fn request_unfreeze(
    ctx: Context<'_>,
    #[description = "Branch to request for"] branch: String
) -> Result<(), Error> {
    let guild_id = match guild_id(ctx) {
        Some(id) => id,
        None => {
            ctx.say(":bangbang: No guild ID!").await?;
            return Ok(());
        }
    };
    match get_frozen(guild_id).await {
        Ok(frozen) => match frozen {
            true => {
                let row = CreateActionRow::default()
                .add_button(
                    CreateButton::default()
                    .label("Approve").to_owned()
                )
                .add_button(CreateButton::default().label("Deny").to_owned()).to_owned();
                ctx.send(|f| f
                    .content(format!(":white_check_mark: Successfully created an unfreeze request for `{}`.", branch))
                    .components(|f| f
                        .add_action_row(row)
                    )
                ).await?
            },
            false => ctx.say(":sunglasses: Your code isn't currently frozen. There's no need to create a request!").await?
        },
        Err(_) => ctx.say(":bangbang: I'm afraid I'm not tracking your code yet. Try using `/freeze` or `/thaw`.").await?
    };
    Ok(())
}

#[poise::command(slash_command)]
#[allow(unused_variables)]
async fn review(
    ctx: Context<'_>,
    #[description = "Branch to request for"] branch: String,
    #[description = "Review to give"] review: String
) -> Result<(), Error> { Ok(()) }

#[tokio::main]
async fn main() {
    dotenv().ok();

    poise::Framework::builder()
        .token(std::env::var("DISCORD_TOKEN").expect("token"))
        .setup(move |_ctx, _ready, _framework: &poise::Framework<_, _>| {
            Box::pin(async move { Ok(Data {}) })
        })
        .options(poise::FrameworkOptions {
            prefix_options: poise::PrefixFrameworkOptions {
                prefix: Some("&".into()),
                ..Default::default()
            },
            commands: vec![freeze(), thaw(), status(), request_unfreeze()],
            ..Default::default()
        })
        .run()
        .await
        .unwrap();
}
