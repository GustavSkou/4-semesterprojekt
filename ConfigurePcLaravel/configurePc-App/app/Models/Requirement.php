<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Requirement extends Model
{
    protected $fillable = [
        'name',
        'value',
        'component_id',
    ];

    public function component()
    {
        return $this->belongsTo(Component::class);
    }
}
