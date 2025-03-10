import { NavLink } from "react-router-dom";
import {z} from "zod";
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import { useState } from "react"
import { Button, Text, Field, Input, Title1 } from "@fluentui/react-components"

const Schema = z.object({
    emailOrUserName: z.string().min(1, {  message: "Write your email or username" }),
    password: z.string().min(1, { message: "Write your password" }),
})

export const SignIn = () => {

    const { register, handleSubmit, formState: { errors, isSubmitting }, reset } = useForm<z.infer<typeof Schema>>({
        resolver: zodResolver(Schema),
        defaultValues: {
            emailOrUserName: "",
            password: "",
        }
    })
    const [httpError, setHttpError] = useState("");

    async function onSubmit(values: z.infer<typeof Schema>) {
            var response = await fetch("/api/Auth/login", {
                method: "POST",
                body: JSON.stringify(values),
                headers: {
                    "Content-Type": "application/json"
                },
                credentials: 'include',
            })
    
            if (!response.ok) {
                var error = await response.json()
                setHttpError(error.message)
    
                setTimeout(() => setHttpError(""), 5000)
            }
            if (response.ok) {
                reset()
                window.location.href = "/angular/";
            }
        }

    return (
        <div style={{ display:' flex', alignItems:'center', flexDirection: 'column', width: '90vw', marginLeft: '5em' }}>
            <div style={{ marginBottom: '1em'}}>
                <Title1>Sign in</Title1>
            </div>
            <form onSubmit={handleSubmit(onSubmit)}>
                {httpError && 
                <p style={{ color: 'red', width: '17em', height: '2em', borderRadius: '0.2em' }}>{httpError}</p>}

                <div style={{ display:'flex', flexDirection: 'column' }}>

                    <Field label={"Email or username"} validationMessage={errors.emailOrUserName?.message}
                                            style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("emailOrUserName")}
                            id="emailOrUserName"
                            placeholder="Email or username">
                        </Input>
                    </Field>

                    <Field label={"Password"} validationMessage={errors.password?.message}
                        style={{ marginBottom: "1em" }}>
                        <Input
                            {...register("password")}
                            type="password"
                            id="password"
                            placeholder="Password">
                        </Input>
                    </Field>
                    
                    <Button disabled={isSubmitting} type="submit" 
                        style={{ marginTop: '1em', backgroundColor: 'green', width: '17em',
                            height: '2.5em'
                         }}>
                        Sign in
                    </Button>
                </div>

                <div style={{ display:'flex', flexDirection: 'column', marginTop: '1em' }}>
                    <Text>Don't have an account?</Text>
                    <NavLink to="/register"> Click here to register.</NavLink> 
                </div>
            </form>
        </div>
    )
}