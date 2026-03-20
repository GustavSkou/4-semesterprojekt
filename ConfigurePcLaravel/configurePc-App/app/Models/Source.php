<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Source extends Model
{
    protected $fillable = [
        'name',
    ];

    public function logs()
    {
        return $this->hasMany(Log::class, 'id', 'source_id');
    }
}
