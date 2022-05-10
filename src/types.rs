use serde::{Serialize, Deserialize};

#[derive(Debug, Serialize, Deserialize)]
pub struct FreezeState {
    pub guild_id: String,
    pub frozen: bool
}

pub struct UnfreezeRequest {
    pub branch_name: String,
    pub requester: String,
    pub approvals: i32
}